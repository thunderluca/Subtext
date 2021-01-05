using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using log4net;
using Subtext.Azure.Search.Models;
using Subtext.Framework.Services.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Search
{
    public class AzureSearchEngineService : ISearchEngineService
    {
        private readonly string _endpoint, _apiKey;
        private readonly ILog _logger;
        private readonly IDictionary<string, SearchClient> _searchClients;

        public AzureSearchEngineService(string apiKey, string endpoint, ILog logger)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException($"{nameof(apiKey)} cannot be null, empty or blank", nameof(apiKey));

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException($"{nameof(endpoint)} cannot be null, empty or blank", nameof(endpoint));

            _apiKey = apiKey;
            _endpoint = endpoint;
            _logger = logger;
            _searchClients = new Dictionary<string, SearchClient>();
        }

        public IEnumerable<IndexingError> AddPost(SearchEngineEntry post)
        {
            EnsureIndexExists(post.BlogId);

            try
            {
                RemovePost(post.EntryId);

                var client = GetSearchClient(post.BlogId);

                var documents = new[]
                {
                    new Entry
                    {
                        BlogId = post.BlogId,
                        BlogName = post.BlogName,
                        Body = post.Body,
                        GroupId = post.GroupId,
                        IsPublished = post.IsPublished,
                        Id = post.EntryId.ToString(),
                        Name = post.EntryName ?? string.Empty,
                        PublishDate = post.PublishDate,
                        Tags = post.Tags,
                        Title = post.Title
                    }
                };

                var response = client.UploadDocuments(documents);
                if (response == null && response.Value == null)
                {
                    _logger?.Warn($"Received null response from merge/upload documents command from blog index with id '{client.IndexName}', not sure if request was successful");
                    return new IndexingError[0];
                }

                return response.Value.Results
                    .Where(r => r.Status != 200 && r.Status != 201)
                    .Select(r => new IndexingError(post, new Exception(r.ErrorMessage)))
                    .ToArray();
            }
            catch (Exception ex)
            {
                return new[] { new IndexingError(post, ex) };
            }
        }

        public IEnumerable<IndexingError> AddPosts(IEnumerable<SearchEngineEntry> posts)
        {
            return AddPosts(posts, true);
        }

        public IEnumerable<IndexingError> AddPosts(IEnumerable<SearchEngineEntry> posts, bool optimize)
        {
            var indexingErrorList = new List<IndexingError>();

            foreach (var post in posts)
            {
                var errors = AddPost(post);
                if (errors != null && errors.Any())
                {
                    indexingErrorList.AddRange(errors);
                }
            }

            return indexingErrorList;
        }

        public void Dispose()
        {
            //Code or nothing
        }

        public int GetIndexedEntryCount(int blogId)
        {
            EnsureIndexExists(blogId);

            var client = GetSearchClient(blogId);

            var documentCount = client.GetDocumentCount();

            return (int)documentCount;
        }

        public int GetTotalIndexedEntryCount()
        {
            var totalCount = 0;

            var searchIndexClient = GetSearchIndexClient();

            var indexes = searchIndexClient.GetIndexes();
            if (indexes == null || !indexes.Any())
            {
                _logger?.Warn($"The index service didn't find any index");
                return totalCount;
            }

            SearchClient client;
            long indexDocumentCount;

            foreach (var index in indexes)
            {
                client = GetSearchClient(index.Name);

                indexDocumentCount = client.GetDocumentCount();

                totalCount += (int)indexDocumentCount;
            }

            return totalCount;
        }

        public IEnumerable<SearchEngineResult> RelatedContents(int entryId, int max, int blogId)
        {
            throw new NotImplementedException();
        }

        public void RemovePost(int postId)
        {
            var searchIndexClient = GetSearchIndexClient();

            var indexes = searchIndexClient.GetIndexes();
            if (indexes == null || !indexes.Any())
            {
                _logger?.Warn($"The index service didn't find any index");
                return;
            }

            SearchClient client;
            string indexNameToUpdate = null;
            SearchOptions options;
            Response<SearchResults<Entry>> response;

            foreach (var index in indexes)
            {
                client = GetSearchClient(index.Name);

                options = new SearchOptions
                {
                    Filter = $"{nameof(Entry.Id)} eq '{postId}'"
                };

                response = client.Search<Entry>("*", options);
                if (response != null && response.Value != null && response.Value.TotalCount > 0)
                {
                    indexNameToUpdate = index.Name;
                    break;
                }
            }

            if (indexNameToUpdate == null)
            {
                _logger?.Warn($"Didn't find any index that contains post with id '{postId}'");
                return;
            }

            client = GetSearchClient(indexNameToUpdate);

            client.DeleteDocuments(keyName: nameof(Entry.Id),
                                   keyValues: new[] { postId.ToString() },
                                   options: new IndexDocumentsOptions { ThrowOnAnyError = true });
        }

        public IEnumerable<SearchEngineResult> Search(string queryString, int max, int blogId)
        {
            return Search(queryString, max, blogId, -1);
        }

        public IEnumerable<SearchEngineResult> Search(string queryString, int max, int blogId, int entryId)
        {
            EnsureIndexExists(blogId);

            var client = GetSearchClient(blogId);

            Response<SearchResults<Entry>> response;

            if (entryId > -1)
            {
                response = client.Search<Entry>(queryString, new SearchOptions
                {
                    Filter = $"{nameof(Entry.Id)} eq '{entryId}'"
                });
            }
            else
            {
                response = client.Search<Entry>(queryString);
            }

            if (response == null || response.Value == null)
            {
                _logger?.Warn($"Received null response from index search with blog id '{blogId}'");
                return new SearchEngineResult[0];
            }

            if (response.Value.TotalCount == 0)
            {
                return new SearchEngineResult[0];
            }

            var results = response.Value.GetResults();

            return results
                .Select(sr => new SearchEngineResult
                {
                    BlogName = sr.Document.BlogName,
                    EntryId = int.Parse(sr.Document.Id),
                    EntryName = sr.Document.Name,
                    PublishDate = sr.Document.PublishDate,
                    Score = sr.Score.HasValue ? Convert.ToSingle(sr.Score.Value) : 0f,
                    Title = sr.Document.Title
                })
                .ToArray();
        }

        private void EnsureIndexExists(int blogId)
        {
            var searchIndexClient = GetSearchIndexClient();

            var searchIndex = GetSearchIndex(blogId);

            searchIndexClient.CreateOrUpdateIndex(searchIndex);
        }

        private SearchIndex GetSearchIndex(int blogId)
        {
            var searchFields = new[]
            {
                new SearchField(nameof(Entry.BlogId), SearchFieldDataType.Int32) { IsFilterable = true },
                new SearchField(nameof(Entry.BlogName), SearchFieldDataType.String) { IsFilterable = true },
                new SearchField(nameof(Entry.Body), SearchFieldDataType.String) { IsFilterable = true },
                new SearchField(nameof(Entry.GroupId), SearchFieldDataType.Int32) { IsFilterable = true },
                new SearchField(nameof(Entry.Id), SearchFieldDataType.String) { IsFilterable = true, IsKey = true },
                new SearchField(nameof(Entry.IsPublished), SearchFieldDataType.Boolean) { IsFilterable = true },
                new SearchField(nameof(Entry.Name), SearchFieldDataType.String) { IsFilterable = true },
                new SearchField(nameof(Entry.PublishDate), SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                new SearchField(nameof(Entry.Tags), SearchFieldDataType.String) { IsFilterable = true },
                new SearchField(nameof(Entry.Title), SearchFieldDataType.String) { IsFilterable = true }
            };

            var searchIndex = new SearchIndex($"blog-{blogId}", searchFields);

            return searchIndex;
        }

        private SearchClient GetSearchClient(int blogId)
        {
            return GetSearchClient($"blog-{blogId}");
        }

        private SearchClient GetSearchClient(string key)
        {
            if (_searchClients.ContainsKey(key))
            {
                return _searchClients[key];
            }

            var client = new SearchClient(new Uri(_endpoint), key, new AzureKeyCredential(_apiKey));

            _searchClients[key] = client;

            return client;
        }

        private SearchIndexClient GetSearchIndexClient()
        {
            var searchIndexClient = new SearchIndexClient(new Uri(_endpoint), new AzureKeyCredential(_apiKey));

            return searchIndexClient;
        }
    }
}

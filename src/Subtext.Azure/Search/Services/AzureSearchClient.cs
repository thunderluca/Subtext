using Azure.Search.Documents;
using log4net;
using Subtext.Azure.Search.Models;
using Subtext.Framework.Services.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Search.Services
{
    public class AzureSearchClient : ISearchClient
    {
        private readonly SearchClient _client;
        private readonly ILog _logger;

        public AzureSearchClient(SearchClient client, ILog logger = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? LogManager.GetLogger(nameof(AzureSearchClient) + "_" + _client.IndexName);
        }

        public bool ContainsEntry(int entryId)
        {
            var response = _client.Search<Entry>("*", new SearchOptions
            {
                Filter = $"{nameof(Entry.Id)} eq '{entryId}'"
            });

            return response != null && response.Value != null && response.Value.TotalCount > 0;
        }

        public long CountEntries()
        {
            return _client.GetDocumentCount();
        }

        public void DeleteEntries(string fieldName, string[] values, bool throwOnError)
        {
            _client.DeleteDocuments(fieldName, values, new IndexDocumentsOptions { ThrowOnAnyError = throwOnError });
        }

        public IEnumerable<SearchEngineResult> Search(string query, int blogId, int size, int entryId = -1)
        {
            var response = _client.Search<Entry>(query, new SearchOptions
            {
                Filter = entryId > -1 ? $"{nameof(Entry.Id)} eq '{entryId}'" : null,
                Size = size
            });

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

        public IEnumerable<IndexingError> UploadEntry(SearchEngineEntry searchEngineEntry)
        {
            var entry = new Entry
            {
                BlogId = searchEngineEntry.BlogId,
                BlogName = searchEngineEntry.BlogName,
                Body = searchEngineEntry.Body,
                GroupId = searchEngineEntry.GroupId,
                IsPublished = searchEngineEntry.IsPublished,
                Id = searchEngineEntry.EntryId.ToString(),
                Name = searchEngineEntry.EntryName ?? string.Empty,
                PublishDate = searchEngineEntry.PublishDate,
                Tags = searchEngineEntry.Tags,
                Title = searchEngineEntry.Title
            };

            var response = _client.UploadDocuments(new[] { entry });
            if (response == null && response.Value == null)
            {
                _logger?.Warn($"Received null response from merge/upload documents command from blog index with id '{_client.IndexName}', not sure if request was successful");
                return new IndexingError[0];
            }

            return response.Value.Results
                .Where(r => r.Status != 200 && r.Status != 201)
                .Select(r => new IndexingError(searchEngineEntry, new Exception(r.ErrorMessage)))
                .ToArray();
        }
    }
}

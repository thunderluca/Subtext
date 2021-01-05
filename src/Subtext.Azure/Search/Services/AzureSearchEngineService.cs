using log4net;
using Subtext.Azure.Search.Models;
using Subtext.Framework.Services.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Search.Services
{
    public class AzureSearchEngineService : ISearchEngineService
    {
        private readonly IIndexFactory _indexFactory;
        private readonly ILog _logger;

        public AzureSearchEngineService(IIndexFactory indexFactory, ILog logger = null)
        {
            _indexFactory = indexFactory ?? throw new ArgumentNullException(nameof(indexFactory));
            _logger = logger ?? LogManager.GetLogger(nameof(AzureSearchEngineService));
        }

        public IEnumerable<IndexingError> AddPost(SearchEngineEntry post)
        {
            if (post == null)
            {
                throw new ArgumentNullException(nameof(post));
            }

            _indexFactory.EnsureIndexExists(post.BlogId);

            this.RemovePost(post.EntryId);

            try
            {
                var client = _indexFactory.GetSearchClient(post.BlogId);

                var errors = client.UploadEntry(post);

                return errors;
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
            if (posts == null)
            {
                throw new ArgumentNullException(nameof(posts));
            }

            if (!posts.Any())
            {
                return new IndexingError[0];
            }

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
            _indexFactory.EnsureIndexExists(blogId);

            var client = _indexFactory.GetSearchClient(blogId);

            var documentCount = client.CountEntries();

            return (int)documentCount;
        }

        public int GetTotalIndexedEntryCount()
        {
            var totalCount = 0;

            var indexNames = _indexFactory.GetIndexNames();
            if (indexNames == null || !indexNames.Any())
            {
                _logger?.Warn($"The indexing service didn't find any index");
                return totalCount;
            }

            ISearchClient client;
            long indexDocumentCount;

            foreach (var indexName in indexNames)
            {
                client = _indexFactory.GetSearchClient(indexName);

                indexDocumentCount = client.CountEntries();

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
            var indexNames = _indexFactory.GetIndexNames();
            if (indexNames == null || !indexNames.Any())
            {
                _logger?.Warn("The indexing service didn't find any index");
                return;
            }

            ISearchClient client;
            string indexNameToUpdate = null;

            foreach (var indexName in indexNames)
            {
                client = _indexFactory.GetSearchClient(indexName);

                var result = client.ContainsEntry(postId);
                if (result)
                {
                    indexNameToUpdate = indexName;
                    break;
                }
            }

            if (indexNameToUpdate == null)
            {
                _logger?.Warn($"Didn't find any index that contains post with id '{postId}'");
                return;
            }

            client = _indexFactory.GetSearchClient(indexNameToUpdate);

            client.DeleteEntries(nameof(Entry.Id), new[] { postId.ToString() }, true);
        }

        public IEnumerable<SearchEngineResult> Search(string queryString, int max, int blogId)
        {
            return Search(queryString, max, blogId, entryId: -1);
        }

        public IEnumerable<SearchEngineResult> Search(string queryString, int max, int blogId, int entryId)
        {
            _indexFactory.EnsureIndexExists(blogId);

            var client = _indexFactory.GetSearchClient(blogId);

            var results = client.Search(queryString, blogId, max, entryId);

            return results;
        }
    }
}

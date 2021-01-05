using log4net;
using Subtext.Azure.Search.Models;
using Subtext.Framework.Services.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Search.Services.Preview
{
    public class AzurePreviewSearchClient : ISearchClient
    {
        private readonly string _endpoint, _indexName, _version;
        private readonly IHttpService _httpService;
        private readonly ILog _logger;
        private readonly ISerializationService _serializationService;

        private const string CURRENT_API_PREVIEW_VERSION = "2020-06-30-Preview";

        public AzurePreviewSearchClient(string endpoint, int blogId, IHttpService httpService, ISerializationService serializationService, ILog logger = null, string version = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException($"{nameof(endpoint)} cannot be null, empty or blank", nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                version = CURRENT_API_PREVIEW_VERSION;
            }

            _endpoint = endpoint;
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _indexName = $"blog-{blogId}";
            _logger = logger;
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            _version = version;
        }

        public bool ContainsEntry(int entryId)
        {
            throw new NotImplementedException();
        }

        public long CountEntries()
        {
            throw new NotImplementedException();
        }

        public void DeleteEntries(string fieldName, string[] values, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SearchEngineResult> Search(string query, int blogId, int size, int entryId = -1)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SearchEngineResult> SearchRelatedContents(int blogId, int size, int entryId)
        {
            var url = $"{_endpoint}/indexes/{_indexName}/docs/search?api-version={_version}&$top={size}";

            var requestContent = _serializationService.Serialize(new Models.Preview.SearchRequest
            {
                Fields = string.Join(",", nameof(Entry.Body), nameof(Entry.Tags), nameof(Entry.Title)),
                MoreLikeThis = entryId.ToString()
            });

            var contentTask = _httpService.PostContentAsync(requestContent, "application/json", url);
            contentTask.Wait();

            if (string.IsNullOrWhiteSpace(contentTask.Result))
            {
                _logger?.Error("Received null or invalid response from moreLikeThis request");
                return new SearchEngineResult[0];
            }

            var response = _serializationService.Deserialize<Models.Preview.SearchResponse>(contentTask.Result);
            if (response == null || response.Value == null)
            {
                return new SearchEngineResult[0];
            }

            return response.Value
                .Select(sr => new SearchEngineResult
                {
                    BlogName = sr.BlogName,
                    EntryId = int.Parse(sr.Id),
                    EntryName = sr.Name,
                    PublishDate = sr.PublishDate,
                    Score = sr.Score.HasValue ? Convert.ToSingle(sr.Score.Value) : 1f,
                    Title = sr.Title
                })
                .ToArray();
        }

        public IEnumerable<IndexingError> UploadEntry(SearchEngineEntry searchEngineEntry)
        {
            throw new NotImplementedException();
        }
    }
}

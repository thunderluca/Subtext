﻿using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Subtext.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subtext.Azure.Search.Services
{
    public class AzureSearchIndexFactory : IIndexFactory
    {
        private readonly IDictionary<string, ISearchClient> _cacheClients;
        private readonly string _endpoint;
        private readonly IHttpService _httpService;
        private readonly SearchIndexClient _indexClient;
        private readonly ISerializationService _serializationService;

        public AzureSearchIndexFactory(string apiKey, string endpoint, IHttpService httpService, ISerializationService serializationService)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException($"{nameof(apiKey)} cannot be null, empty or blank", nameof(apiKey));
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException($"{nameof(endpoint)} cannot be null, empty or blank", nameof(endpoint));
            }

            _cacheClients = new Dictionary<string, ISearchClient>();
            _endpoint = endpoint;
            _httpService = httpService ?? throw new ArgumentNullException(nameof(httpService));
            _indexClient = new SearchIndexClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        public void EnsureIndexExists(int blogId)
        {
            var searchIndex = GetSearchIndex(blogId);

            _indexClient.CreateOrUpdateIndex(searchIndex);
        }

        public IEnumerable<string> GetIndexNames()
        {
            var indexes = _indexClient.GetIndexes();
            if (indexes == null || !indexes.Any())
            {
                return new string[0];
            }

            return indexes.Select(i => i.Name).ToArray();
        }

        public ISearchClient GetPreviewSearchClient(int blogId)
        {
            var client = new Preview.AzurePreviewSearchClient(_endpoint, blogId, _httpService, _serializationService);

            return client;
        }

        public ISearchClient GetSearchClient(int blogId)
        {
            return GetSearchClient($"blog-{blogId}");
        }

        public ISearchClient GetSearchClient(string indexName)
        {
            if (_cacheClients.ContainsKey(indexName))
            {
                return _cacheClients[indexName];
            }

            var searchClient = _indexClient.GetSearchClient(indexName);

            var client = new AzureSearchClient(searchClient);

            _cacheClients[indexName] = client;

            return client;
        }

        private SearchIndex GetSearchIndex(int blogId)
        {
            var searchFields = new[]
            {
                new SearchField(nameof(Entry.BlogId), SearchFieldDataType.Int32) { IsFilterable = true },
                new SimpleField(nameof(Entry.BlogName), SearchFieldDataType.String),
                new SearchField(nameof(Entry.Body), SearchFieldDataType.String) { IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene },
                new SimpleField(nameof(Entry.GroupId), SearchFieldDataType.Int32),
                new SearchField(nameof(Entry.Id), SearchFieldDataType.String) { IsFilterable = true, IsKey = true },
                new SearchField(nameof(Entry.IsPublished), SearchFieldDataType.Boolean) { IsFilterable = true },
                new SimpleField(nameof(Entry.Name), SearchFieldDataType.String),
                new SearchField(nameof(Entry.PublishDate), SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                new SearchField(nameof(Entry.Tags), SearchFieldDataType.String) { IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene },
                new SearchField(nameof(Entry.Title), SearchFieldDataType.String) { IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene }
            };

            var searchIndex = new SearchIndex($"blog-{blogId}", searchFields);

            return searchIndex;
        }
    }
}

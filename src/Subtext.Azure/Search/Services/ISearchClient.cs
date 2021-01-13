﻿using Subtext.Framework.Services.SearchEngine;
using System.Collections.Generic;

namespace Subtext.Azure.Search.Services
{
    public interface ISearchClient
    {
        bool ContainsEntry(int entryId);

        long CountEntries(int blogId = -1);

        void DeleteEntries(string fieldName, string[] values, bool throwOnError);

        IEnumerable<SearchEngineResult> Search(string query, int blogId, int size, int entryId = -1);

        IEnumerable<SearchEngineResult> SearchRelatedContents(int blogId, int size, int entryId);

        IEnumerable<IndexingError> UploadEntry(SearchEngineEntry searchEngineEntry);
    }
}
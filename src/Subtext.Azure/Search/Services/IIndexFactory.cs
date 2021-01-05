using System.Collections.Generic;

namespace Subtext.Azure.Search.Services
{
    public interface IIndexFactory
    {
        void EnsureIndexExists(int blogId);

        IEnumerable<string> GetIndexNames();

        ISearchClient GetPreviewSearchClient(int blogId);

        ISearchClient GetSearchClient(int blogId);

        ISearchClient GetSearchClient(string indexName);
    }
}

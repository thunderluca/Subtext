using System.Collections.Generic;

namespace Subtext.Azure.Search.Services
{
    public interface IIndexFactory
    {
        void EnsureIndexExists();

        IEnumerable<string> GetIndexNames();

        ISearchClient GetPreviewSearchClient();

        ISearchClient GetSearchClient();
    }
}

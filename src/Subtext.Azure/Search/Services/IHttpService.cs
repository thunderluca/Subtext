using System.Threading.Tasks;

namespace Subtext.Azure.Search.Services
{
    public interface IHttpService
    {
        Task<string> PostContentAsync(string content, string mimeType, string url);
    }
}

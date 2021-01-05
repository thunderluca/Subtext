using Subtext.Azure.Search.Services;
using System.Text.Json;

namespace Subtext.Web.Services
{
    public class SystemSerializationService : ISerializationService
    {
        public T Deserialize<T>(string content)
        {
            return JsonSerializer.Deserialize<T>(content);
        }

        public string Serialize<T>(T item)
        {
            return JsonSerializer.Serialize(item);
        }
    }
}
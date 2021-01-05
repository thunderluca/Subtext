using System.Text.Json.Serialization;

namespace Subtext.Azure.Search.Models.Preview
{
    public class SearchResponse
    {
        [JsonPropertyName("value")]
        public SearchEntry[] Value { get; set; }
    }
}

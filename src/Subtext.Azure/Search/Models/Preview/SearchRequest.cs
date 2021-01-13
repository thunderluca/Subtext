using System.Text.Json.Serialization;

namespace Subtext.Azure.Search.Models.Preview
{
    public class SearchRequest
    {
        [JsonPropertyName("searchFields")]
        public string Fields { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("moreLikeThis")]
        public string MoreLikeThis { get; set; }
    }
}

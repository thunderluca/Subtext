using System;
using System.Text.Json.Serialization;

namespace Subtext.Azure.Search.Models.Preview
{
    public class SearchEntry
    {
        [JsonPropertyName("BlogId")]
        public int BlogId { get; set; }

        [JsonPropertyName("BlogName")]
        public string BlogName { get; set; }

        [JsonPropertyName("Body")]
        public string Body { get; set; }

        [JsonPropertyName("GroupId")]
        public int GroupId { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("IsPublished")]
        public bool IsPublished { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("PublishDate")]
        public DateTime PublishDate { get; set; }

        [JsonPropertyName("@search.score")]
        public double? Score { get; set; }

        [JsonPropertyName("Tags")]
        public string Tags { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }
    }
}

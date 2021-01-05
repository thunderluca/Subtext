using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System;

namespace Subtext.Azure.Search.Models
{
    public class Entry
    {
        [SearchableField(IsFilterable = true)]
        public int BlogId { get; set; }

        [SimpleField]
        public string BlogName { get; set; }

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
        public string Body { get; set; }

        [SimpleField]
        public int GroupId { get; set; }

        [SearchableField(IsFilterable = true, IsKey = true)]
        public string Id { get; set; }

        [SearchableField(IsFilterable = true)]
        public bool IsPublished { get; set; }

        [SimpleField]
        public string Name { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public DateTime PublishDate { get; set; }

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
        public string Tags { get; set; }

        [SearchableField(IsFilterable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
        public string Title { get; set; }
    }
}

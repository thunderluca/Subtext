using Azure.Search.Documents.Indexes;
using System;

namespace Subtext.Azure.Search.Models
{
    public class Entry
    {
        [SearchableField(IsFilterable = true)]
        public int BlogId { get; set; }

        [SearchableField(IsFilterable = true)]
        public string BlogName { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Body { get; set; }

        [SearchableField(IsFilterable = true)]
        public int GroupId { get; set; }

        [SearchableField(IsFilterable = true, IsKey = true)]
        public string Id { get; set; }

        [SearchableField(IsFilterable = true)]
        public bool IsPublished { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Name { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public DateTime PublishDate { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Tags { get; set; }

        [SearchableField(IsFilterable = true)]
        public string Title { get; set; }
    }
}

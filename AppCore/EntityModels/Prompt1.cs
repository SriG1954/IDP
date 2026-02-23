using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
     public partial class Prompt1
    {
        public int PromptId { get; set; }
        public string Name { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class ModelEndpoint
    {
        public int EndpointId { get; set; }
        public string Name { get; set; } = default!;
        public string EndpointName { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string ContentType { get; set; } = "application/json";
        public string? Accept { get; set; }
        public int? MaxTokens { get; set; }
        public decimal? Temperature { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class WorktypeCatalog
    {
        public int CatalogId { get; set; }
        public string JsonContent { get; set; } = default!;
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class PhraseOverride
    {
        public int OverrideId { get; set; }
        public string Phrase { get; set; } = default!;
        public string Worktype { get; set; } = default!;
        public string Subworktype { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class CheckboxFlagRule
    {
        public int RuleId { get; set; }
        public string FlagType { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string? SectionKeywords { get; set; } // JSON array
        public string? LabelAny { get; set; }        // JSON array
        public string? LabelNone { get; set; }       // JSON array
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class DocumentJob
    {
        public long JobId { get; set; }
        public string ChannelType { get; set; } = "Post";
        public string S3Bucket { get; set; } = default!;
        public string S3Key { get; set; } = default!;
        public int MaxPages { get; set; } = 4;
        public int KvPagesLimit { get; set; } = 4;
        public string Status { get; set; } = "Queued";
        public int Priority { get; set; } = 0;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
        public string? Error { get; set; }

        //public DocumentResult? Result { get; set; }
    }

    public partial class DocumentResult
    {
        public long JobId { get; set; }
        public string ClassificationJson { get; set; } = default!;
        public string? StructuredFlagsJson { get; set; }
        public string? CatalogScoresJson { get; set; }
        public string? TimingsJson { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        //public DocumentJob Job { get; set; } = default!;
    }
}

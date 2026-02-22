using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Models
{
    // AppCore/Models/Domain.cs
    public sealed class Document
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; } = default!;   // e.g., MessageId/AttachmentId
        public string FileName { get; set; } = default!;
        public string MimeType { get; set; } = default!;
        public string StoragePath { get; set; } = default!;  // local folder or s3://...
        public long SizeBytes { get; set; }
        public DateTimeOffset IngestedAt { get; set; }
        public ICollection<ClassificationResult> Classifications { get; set; } = new List<ClassificationResult>();
        public ICollection<KVPair> KeyValues { get; set; } = new List<KVPair>();
    }

    public sealed class ClassificationResult
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Document Document { get; set; } = default!;
        public string Label { get; set; } = default!;
        public double Confidence { get; set; }
        public string ModelName { get; set; } = default!;
        public string ModelVersion { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsFinal { get; set; }            // after routing / human review
        public string? TraceId { get; set; }         // for distributed tracing/log correlation
    }

    public sealed class KVPair
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Document Document { get; set; } = default!;
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string Source { get; set; } = default!;  // "Textract", "Regex", "LLM"
        public double? Confidence { get; set; }
    }

    public sealed class Prompt
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;       // e.g., "DocClassification.v3"
        public string Template { get; set; } = default!;   // prompt text
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class KvExtractionRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;       // e.g., "Invoice/TotalAmount"
        public string Strategy { get; set; } = default!;   // "TextractQuery" | "Regex" | "Heuristic"
        public string RuleJson { get; set; } = default!;   // payload (e.g., Textract Queries)
        public int Version { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class RegexRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Pattern { get; set; } = default!;
        public string Options { get; set; } = "IgnoreCase|Compiled"; // persisted options
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(1);
        public string TargetField { get; set; } = default!; // e.g., "InvoiceNumber"
        public int Version { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class AdaptiveLearningRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string ConditionJson { get; set; } = default!;  // e.g., "if Vendor=X then label as Y"
        public string ActionJson { get; set; } = default!;
        public bool IsActive { get; set; }
        public int Version { get; set; }
    }

    public sealed class ModelThreshold
    {
        public int Id { get; set; }
        public string Label { get; set; } = default!;          // class label or "default"
        public double AutoAcceptThreshold { get; set; } = 0.92;
        public double HumanReviewThreshold { get; set; } = 0.70;
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public sealed class AgentRun
    {
        public long Id { get; set; }
        public string AgentName { get; set; } = default!;
        public string? CorrelationId { get; set; }
        public Guid? DocumentId { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? EndedAt { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}

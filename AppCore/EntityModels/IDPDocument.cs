using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPDocument
    {
        public long DocumentId { get; set; }

        public long BatchId { get; set; }

        public string FileName { get; set; } = null!;

        public string FilePath { get; set; } = null!;

        public string? FileHash { get; set; }

        public string? MimeType { get; set; }

        public long? FileSizeBytes { get; set; }

        public DocumentStatus DocumentStatus { get; set; }

        public int RetryCount { get; set; }

        public decimal? ConfidenceScore { get; set; }

        public string? FinalClassification { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }
    }
}

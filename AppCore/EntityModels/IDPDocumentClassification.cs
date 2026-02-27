using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPDocumentClassification
    {
        public long ClassificationId { get; set; }

        public long DocumentId { get; set; }

        public ClassificationSource ClassificationSource { get; set; }

        public string PredictedLabel { get; set; } = null!;

        public decimal? ConfidenceScore { get; set; }

        public bool IsFinal { get; set; }

        public string? ModelVersion { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

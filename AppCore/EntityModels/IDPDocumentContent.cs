using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPDocumentContent
    {
        public long DocumentId { get; set; }

        public string? OcrText { get; set; }

        public string? ExtractedKvJson { get; set; }

        public string? RawLlmResponse { get; set; }

        public string? RawMlFeatures { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Models
{

    public sealed class AgentContext
    {
        public Guid? DocumentId { get; init; }
        public string? FilePath { get; init; }
        public string? MimeType { get; init; }
        public byte[]? ContentBytes { get; init; }
        public string? CorrelationId { get; init; }
        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    }

    public sealed class AgentResult
    {
        public bool Success { get; init; }
        public string? Error { get; init; }
        public IDictionary<string, object> Outputs { get; } = new Dictionary<string, object>();
    }

}

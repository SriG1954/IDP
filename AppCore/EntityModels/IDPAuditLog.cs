using AppCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPAuditLog
    {
        public long LogId { get; set; }

        public int? EntityId { get; set; }

        public long? BatchId { get; set; }

        public long? BulkCopyId { get; set; }

        public long? DeleteId { get; set; }

        public AuditEventType EventType { get; set; }

        public string Message { get; set; } = null!;

        public AuditLogLevel AuditLogLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }

}

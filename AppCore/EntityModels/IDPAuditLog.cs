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

        public long? DocumentId { get; set; }

        public AuditEventType EventType { get; set; }

        public string Message { get; set; } = null!;

        public AuditLogLevel AuditLogLevel { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }

    public enum AuditLogLevel
    {
        Trace = 0,      // Detailed, debug info
        Info = 1,       // Normal process events
        Warning = 2,    // Recoverable issues, retries
        Error = 3,      // Failures requiring attention
        Critical = 4    // Process-stopping issues
    }

    public enum AuditEventType
    {
        System = 0,
        Testing = 1,
        FetchEmailMessage = 2,
        AWSContext = 3,
        TextractContext = 4,
        SQLContext = 5,
        S3Context = 6,
        HardDelet = 7,
        HDReverse = 8,
        HDVerifiy = 9,
        HDService = 11,
    }

}

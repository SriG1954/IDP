using AppCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{

    public interface IAgent
    {
        string Name { get; }
        Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken ct);
    }

    public enum BatchStatus
    {
        New = -1,                   // Just created, not yet processed
        Pending = 0,                // Ready to be processed
        FetchMetaInProgress = 1,    // Fetching Oracle Source meta data in progress
        FetchMetaCompleted = 2,     // Fetching Oracle Source meta data completed
        FetchMetaError = 3,         // Fetching Oracle Source meta data errored
        DeleteInProgress = 5,       // Hard delete In Progress
        DeleteCompleted = 6,        // Hard delete completed
        DeleteError = 7,            // Hard delete errored
        VerifyInProgress = 10,      // Hard delete verify in progress
        DeleteVerified = 11,        // Hard delete verified
        VerifiedError = 12,         // Hard delete verified error
        Reversed = 15,              // Reversed
        ReverseError = 16,          // Reversed error

    }

    public enum MetaStatus
    {
        Fetched = 0,           // Ready to be processed
        HardDeleted = 1,       // Hard Deleted
        DeleteVerified = 2,    // Delete Verified
        SourceDeleted = 3,     // Source is Deleted 
        Errored = 100          // Error occurred, eligible for retry
    }

    public enum BulkCopyStatus
    {
        New = -1,              // Just created, not yet processed
        Pending = 0,           // Ready to be processed
        InProgress = 1,        // In progress
        Completed = 2,         // Successfully completed
        Failed = 99            // Error occurred, eligible for retry
    }

    public enum DeleteAuditStatus
    {
        New = -1,              // Just created, not yet processed
        Pending = 0,           // Ready to be processed
        InProgress = 1,        // In progress
        DeleteCompleted = 2,   // Successfully delete completed
        DeleteError = 3,       // Hard delete errored
        DeleteVerified = 5,    // Successfully delete verified
        VerifiedError = 6,     // Hard delete verified error
        Reversed = 15,         // Reversed
        ReverseError = 16,     // Reversed error

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

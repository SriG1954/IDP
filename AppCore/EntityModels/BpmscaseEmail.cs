using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class BpmscaseEmail
{
    public int Id { get; set; }

    public string? EmailId { get; set; }

    public string? BpmscaseId { get; set; }

    public string? EmailSubject { get; set; }

    public string? EmailSource { get; set; }

    public string? SenderEmail { get; set; }

    public string? SenderName { get; set; }

    public DateTime ReceivedDatetime { get; set; }

    public string? EmailBody { get; set; }

    public bool HasAttachments { get; set; }

    public string? CaseStatus { get; set; }

    public string? CaseStatusMessage { get; set; }

    public int? ItemNumber { get; set; }

    public string? BatchExternalId { get; set; }

    public string? DocumentSource { get; set; }

    public string? CreateCase { get; set; }

    public string? CaseTarget { get; set; }

    public string? WorkType { get; set; }

    public string? SubWorkType { get; set; }

    public string? IndexingSource { get; set; }

    public string? TargetLocation { get; set; }

    public string? BpmscaseStatus { get; set; }

    public string? BatchFolder { get; set; }

    public string? BatchName { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}

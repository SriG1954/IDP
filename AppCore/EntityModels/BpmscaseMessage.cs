using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class BpmscaseMessage
{
    public int Id { get; set; }

    public string MessageId { get; set; } = null!;

    public string? BpmscaseId { get; set; }

    public string? MessageSource { get; set; }

    public string? SenderSource { get; set; }

    public string? SenderName { get; set; }

    public DateTime ReceivedDatetime { get; set; }

    public string? MessageBody { get; set; }

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

    public string? SourceAliasEmail { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}

using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class CmscaseUpdateMessage
{
    public int Id { get; set; }

    public string? MessageId { get; set; }

    public string MessageSource { get; set; } = null!;

    public bool HasAttachments { get; set; }

    public string ClaimNumber { get; set; } = null!;

    public string PolicyNumber { get; set; } = null!;

    public string? CaseStatus { get; set; }

    public string? CaseStatusMessage { get; set; }

    public string? CreateCase { get; set; }

    public string? TargetLocation { get; set; }

    public string? BatchFolder { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}

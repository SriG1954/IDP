using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class CmsexceptionMailbox
{
    public int Sno { get; set; }

    public string? MessageId { get; set; }

    public string? CaseSource { get; set; }

    public string? CaseStatus { get; set; }

    public string? CaseStatusMessage { get; set; }

    public string? ExceptionReason { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}

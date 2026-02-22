using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class CmspostProcessingLog
{
    public int Sno { get; set; }

    public string? MessageId { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public string? MessageSource { get; set; }

    public string? CreateCase { get; set; }

    public string? TargetLocation { get; set; }

    public string? BatchFolder { get; set; }

    public string? ClaimNumber { get; set; }

    public string? PolicyNumber { get; set; }

    public string? Cmsdoctype { get; set; }

    public DateTime? CreatedDateTime { get; set; }

    public DateTime? UpdatedDateTime { get; set; }
}

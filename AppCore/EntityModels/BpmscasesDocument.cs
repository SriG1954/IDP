using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class BpmscasesDocument
{
    public long Id { get; set; }

    public string MessageId { get; set; } = null!;

    public int? BpmscaseId { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public string? DocType { get; set; }

    public string? FileStatus { get; set; }

    public string? FileStatusMessage { get; set; }

    public int? DocumentNumber { get; set; }

    public string? ArsdocumentId { get; set; }

    public string? DocumentId { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }
}

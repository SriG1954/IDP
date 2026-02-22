using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class ConvergePhysicalDocument
{
    public string DocumentId { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string DocumentPath { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string? DocumentSftpinterface { get; set; }

    public int? DocumentSize { get; set; }

    public string TypeOfDocument { get; set; } = null!;

    public string DocumentSource { get; set; } = null!;

    public string ProcessingStatus { get; set; } = null!;

    public string? ProcessingStatusMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

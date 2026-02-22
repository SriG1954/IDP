using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class Ocrdocument
{
    public long Id { get; set; }

    public string MessageId { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public string DocumentPath { get; set; } = null!;

    public string? DocumentPathType { get; set; }

    public long? DocumentSizeBytes { get; set; }

    public int? DocumentDpihorizontal { get; set; }

    public int? DocumentDpivertical { get; set; }

    public int? DocumentHeightPixel { get; set; }

    public int? DocumentWidthPixel { get; set; }

    public string Ocrstatus { get; set; } = null!;

    public string? OcrstatusMessage { get; set; }

    public string? DocumentId { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}

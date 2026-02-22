using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcrtextractBlock
{
    public long Id { get; set; }

    public string? InputFilePath { get; set; }

    public string BlockId { get; set; } = null!;

    public string? BlockType { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public string? Text { get; set; }

    public int? RowIndex { get; set; }

    public int? ColumnIndex { get; set; }

    public int? RowSpan { get; set; }

    public int? ColumnSpan { get; set; }

    public int? PageNumber { get; set; }

    public string? TextType { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

    public string? DocumentId { get; set; }
}

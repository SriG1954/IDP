using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcrtextractWord
{
    public int WordId { get; set; }

    public string? Id { get; set; }

    public string? BlockType { get; set; }

    public string? Confidence { get; set; }

    public string? Ocrtext { get; set; }

    public string? OcrtextType { get; set; }

    public string? Geometry { get; set; }
}

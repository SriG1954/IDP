using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class TextractBlock
{
    public string? Id { get; set; }

    public string? BlockType { get; set; }

    public string? Confidence { get; set; }

    public string? Text { get; set; }

    public string? Geometry { get; set; }

    public string? Relationships { get; set; }
}

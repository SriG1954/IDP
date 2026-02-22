using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcroutputFromLlm
{
    public long JsonId { get; set; }

    public string? InputFilePath { get; set; }

    public string? JsonString { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}

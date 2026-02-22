using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcrtextractJsonDocument
{
    public int JsonId { get; set; }

    public string? Ocrtype { get; set; }

    public string? JsonData { get; set; }
}

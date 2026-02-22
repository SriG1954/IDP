using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class Aillmresponse
{
    public int Id { get; set; }

    public string? PayloadToLlm { get; set; }

    public string? OutputContent { get; set; }
}

using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class BpmsworkTypeMapping
{
    public int Id { get; set; }

    public string SourceEmail { get; set; } = null!;

    public string WorkType { get; set; } = null!;

    public string SubWorkType { get; set; } = null!;

    public string QueueCode { get; set; } = null!;

    public string DocType { get; set; } = null!;
}

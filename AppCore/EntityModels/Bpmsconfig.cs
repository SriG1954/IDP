using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class Bpmsconfig
{
    public int Id { get; set; }

    public int ConfigId { get; set; }

    public string ConfigName { get; set; } = null!;

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public bool? IsActive { get; set; }
}

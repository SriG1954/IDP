using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class MailErrorOutput
{
    public long Id { get; set; }

    public string ErrorMessage { get; set; } = null!;
}

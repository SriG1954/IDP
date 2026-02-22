using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class MailboxConfig
{
    public long Id { get; set; }

    public string MailboxAddress { get; set; } = null!;

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? TenantId { get; set; }

    public bool? IsActive { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppCore.EntityModels;

public partial class MailRecipient
{
    [Key] public long MailRecipientId { get; set; }
    public long MailMessageId { get; set; }

    // "To" | "Cc" | "Bcc" (Bcc for received mails is usually not present)
    [Required, MaxLength(10)] public string Type { get; set; } = default!;
    [MaxLength(256)] public string? DisplayName { get; set; }
    [Required, MaxLength(320)] public string EmailAddress { get; set; } = default!;
}

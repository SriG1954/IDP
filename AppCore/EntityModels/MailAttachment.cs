using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppCore.EntityModels;

public partial class MailAttachment
{
    [Key] public long MailAttachmentId { get; set; }
    public long MailMessageId { get; set; }
    public MailMessage MailMessage { get; set; } = default!;

    [Required, MaxLength(128)] public string GraphAttachmentId { get; set; } = default!;
    [MaxLength(400)] public string? Name { get; set; }
    [MaxLength(200)] public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public bool IsInline { get; set; }

    [MaxLength(1024)] public string? SavedPath { get; set; }
    [MaxLength(64)] public string? Sha256Hex { get; set; }
}

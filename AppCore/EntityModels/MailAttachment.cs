using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppCore.EntityModels;

public partial class MailAttachment
{
    public long MailAttachmentId { get; set; }
    public long MailMessageId { get; set; }
    public string GraphAttachmentId { get; set; } = default!;
    public string? Name { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public bool IsInline { get; set; }

    public string? SavedPath { get; set; }
    public string? Sha256Hex { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCore.EntityModels;

public partial class MailMessage
{
    [Key] public long MailMessageId { get; set; }

    [Required, MaxLength(256)]
    public string Mailbox { get; set; } = default!;         // source mailbox UPN

    [Required, MaxLength(128)]
    public string GraphMessageId { get; set; } = default!;

    [MaxLength(255)]
    public string? InternetMessageId { get; set; }

    [MaxLength(500)]
    public string? Subject { get; set; }

    [MaxLength(320)]
    public string? FromAddress { get; set; }

    [MaxLength(320)]
    public string? SenderAddress { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime ReceivedDateTimeUtc { get; set; }

    [MaxLength(20)]
    public string? Importance { get; set; }

    public bool HasAttachments { get; set; }
    [MaxLength(128)] public string? ConversationId { get; set; }
    public string? BodyPreview { get; set; }

    [Timestamp] public byte[]? RowVersion { get; set; }

    public ICollection<MailHeader> Headers { get; set; } = new List<MailHeader>();
    public ICollection<MailRecipient> Recipients { get; set; } = new List<MailRecipient>();
    public ICollection<MailAttachment> Attachments { get; set; } = new List<MailAttachment>();
}

public class MailSyncState
{
    [Key] public long MailSyncStateId { get; set; }

    [Required, MaxLength(256)] public string Mailbox { get; set; } = default!;
    [Required, MaxLength(128)] public string FolderId { get; set; } = "inbox";
    [Column(TypeName = "date")] public DateTime DateUtc { get; set; }  // store as date

    public string? OdataNextLink { get; set; }

    [Column(TypeName = "datetime2")] public DateTime UpdatedAtUtc { get; set; }
}
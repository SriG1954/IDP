using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCore.EntityModels;

public partial class MailMessage1
{
    public long MailMessageId { get; set; }

    public string Mailbox { get; set; } = default!;         // source mailbox UPN

    public string GraphMessageId { get; set; } = default!;

    public string? InternetMessageId { get; set; }

    public string? Subject { get; set; }

    public string? FromAddress { get; set; }

    public string? SenderAddress { get; set; }

    public DateTime ReceivedDateTimeUtc { get; set; }

    public string? Importance { get; set; }

    public bool HasAttachments { get; set; }

    public string? ConversationId { get; set; }
    public string? BodyPreview { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    //[Timestamp] public byte[]? RowVersion { get; set; }

    //public ICollection<MailHeader> Headers { get; set; } = new List<MailHeader>();
    //public ICollection<MailRecipient> Recipients { get; set; } = new List<MailRecipient>();
    //public ICollection<MailAttachment> Attachments { get; set; } = new List<MailAttachment>();
}

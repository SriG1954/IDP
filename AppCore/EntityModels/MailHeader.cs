using System.ComponentModel.DataAnnotations;

namespace AppCore.EntityModels;

public partial class MailHeader
{
    [Key] public long MailHeaderId { get; set; }
    public long MailMessageId { get; set; }
    public MailMessage MailMessage { get; set; } = default!;

    [Required, MaxLength(256)] public string Name { get; set; } = default!;
    public string? Value { get; set; }
}

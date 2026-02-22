using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class MailClaimNumber
{
    public int Id { get; set; }

    public string? MessageId { get; set; }

    public string? DocumentId { get; set; }

    public string ClaimNumber { get; set; } = null!;

    public decimal? Confidence { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }
}

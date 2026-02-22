using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class BpmsrequestId
{
    public int Id { get; set; }

    public string? MessageId { get; set; }

    public string? DocumentId { get; set; }

    public string RequestId { get; set; } = null!;

    public decimal? Confidence { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public DateTime UpdatedDateTime { get; set; }
}

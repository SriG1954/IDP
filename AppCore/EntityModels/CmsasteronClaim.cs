using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class CmsasteronClaim
{
    public long Id { get; set; }

    public string ClaimNumber { get; set; } = null!;

    public string PolicyNumber { get; set; } = null!;

    public string? CustomerFirstName { get; set; }

    public string? CustomerLastName { get; set; }

    public string? State { get; set; }

    public string? Postcode { get; set; }

    public string? ClaimsTeamDepartment { get; set; }
}

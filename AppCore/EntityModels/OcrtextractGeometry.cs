using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcrtextractGeometry
{
    public long GeometryId { get; set; }

    public string? InputFilePath { get; set; }

    public string? BlockId { get; set; }

    public decimal? BoundingBoxLeft { get; set; }

    public decimal? BoundingBoxTop { get; set; }

    public decimal? BoundingBoxWidth { get; set; }

    public decimal? BoundingBoxHeight { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

    public string? DocumentId { get; set; }
}

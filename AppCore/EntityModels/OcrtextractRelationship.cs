using System;
using System.Collections.Generic;

namespace AppCore.EntityModels;

public partial class OcrtextractRelationship
{
    public long RelationshipId { get; set; }

    public string? InputFilePath { get; set; }

    public string? BlockId { get; set; }

    public string? RelationType { get; set; }

    public string? RelatedBlockId { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

    public string? DocumentId { get; set; }
}

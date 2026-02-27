using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPHumanReview
    {
        public long ReviewId { get; set; }

        public long DocumentId { get; set; }

        public string? PredictedLabel { get; set; }

        public string? CorrectLabel { get; set; }

        public ReviewStatus ReviewStatus { get; set; }

        public string? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }
    }
}

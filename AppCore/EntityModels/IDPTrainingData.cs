using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPTrainingData
    {
        public long TrainingId { get; set; }

        public long DocumentId { get; set; }

        public string OcrText { get; set; } = null!;

        public string? KvJson { get; set; }

        public ChannelType Channel { get; set; }

        public string Label { get; set; } = null!;

        public LabelSource LabelSource { get; set; }

        public DatasetType DatasetType { get; set; }

        public int FeatureVersion { get; set; }

        public string? UsedInModelVersion { get; set; }

        public bool UsedInModel { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}

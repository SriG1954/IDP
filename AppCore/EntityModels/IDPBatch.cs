using AppCore.Interfaces;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.EntityModels
{
    public partial class IDPBatch
    {
        public long BatchId { get; set; }

        public ChannelType ChannelType { get; set; }

        public string? SourceReference { get; set; }

        public BatchStatus BatchStatus { get; set; }

        public int Priority { get; set; } = 5;

        public int TotalDocuments { get; set; }

        public int ProcessedDocuments { get; set; }

        public int FailedDocuments { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public string? CreatedBy { get; set; }
    }

    public enum ChannelType
    {
        Email = 1,
        Post = 2,
        BPMS = 3,
        Other = 4
    }

    public enum BatchStatus
    {
        Created = 1,
        Downloading = 2,
        DownloadCompleted = 3,
        Processing = 4,
        Completed = 5,
        CompletedWithErrors = 6,
        Failed = 7
    }

    public enum DocumentStatus
    {
        DownloadPending = 1,
        Downloaded = 2,
        DownloadFailed = 3,

        OcrPending = 10,
        OcrCompleted = 11,
        OcrFailed = 12,

        KvPending = 20,
        KvCompleted = 21,
        KvFailed = 22,

        ClassificationPending = 30,
        ClassificationCompleted = 31,
        ClassificationLowConfidence = 32,
        ClassificationFailed = 33,

        HumanReviewPending = 40,
        HumanReviewInProgress = 41,
        HumanReviewed = 42,

        Finalized = 50,
        DeadLetter = 99
    }

    public enum ClassificationSource
    {
        LLM = 1,
        ML = 2,
        Human = 3
    }

    public enum ReviewStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Rejected = 4
    }

    public enum LabelSource
    {
        Human = 1,
        LLMConfirmed = 2
    }

    public enum DatasetType
    {
        Train = 1,
        Validation = 2,
        Test = 3
    }
}

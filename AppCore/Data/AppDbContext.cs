using AppCore.EntityModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace AppCore.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IDPAuditLog> IDPAuditLogs { get; set; }

    public virtual DbSet<Aillmresponse> Aillmresponses { get; set; }

    public virtual DbSet<BpmscaseEmail> BpmscaseEmails { get; set; }

    public virtual DbSet<BpmscaseMessage> BpmscaseMessages { get; set; }

    public virtual DbSet<BpmscasesDocument> BpmscasesDocuments { get; set; }

    public virtual DbSet<Bpmsconfig> Bpmsconfigs { get; set; }

    public virtual DbSet<BpmspolicyNumber> BpmspolicyNumbers { get; set; }

    public virtual DbSet<BpmsrequestId> BpmsrequestIds { get; set; }

    public virtual DbSet<BpmsworkTypeMapping> BpmsworkTypeMappings { get; set; }

    public virtual DbSet<CmsasteronClaim> CmsasteronClaims { get; set; }

    public virtual DbSet<CmscaseDocument> CmscaseDocuments { get; set; }

    public virtual DbSet<CmscaseUpdateMessage> CmscaseUpdateMessages { get; set; }

    public virtual DbSet<Cmsconfig> Cmsconfigs { get; set; }

    public virtual DbSet<CmsexceptionMailbox> CmsexceptionMailboxes { get; set; }

    public virtual DbSet<CmspostProcessingLog> CmspostProcessingLogs { get; set; }

    public virtual DbSet<ConvergaDocumentsQueue> ConvergaDocumentsQueues { get; set; }

    public virtual DbSet<ConvergePhysicalDocument> ConvergePhysicalDocuments { get; set; }

    public virtual DbSet<MailClaimNumber> MailClaimNumbers { get; set; }

    public DbSet<MailMessage1> MailMessages => Set<MailMessage1>();
    public DbSet<MailHeader> MailHeaders => Set<MailHeader>();
    public DbSet<MailRecipient> MailRecipients => Set<MailRecipient>();
    public DbSet<MailAttachment> MailAttachments => Set<MailAttachment>();
    public DbSet<MailSyncState> MailSyncStates => Set<MailSyncState>();

    public virtual DbSet<MailErrorOutput> MailErrorOutputs { get; set; }

    public virtual DbSet<MailPolicyNumber> MailPolicyNumbers { get; set; }

    public virtual DbSet<MailboxConfig> MailboxConfigs { get; set; }

    public virtual DbSet<Ocrdocument> Ocrdocuments { get; set; }

    public virtual DbSet<OcrjsonForLlm> OcrjsonForLlms { get; set; }

    public virtual DbSet<OcroutputFromLlm> OcroutputFromLlms { get; set; }

    public virtual DbSet<OcrtextractBlock> OcrtextractBlocks { get; set; }

    public virtual DbSet<OcrtextractGeometry> OcrtextractGeometries { get; set; }

    public virtual DbSet<OcrtextractJsonDocument> OcrtextractJsonDocuments { get; set; }

    public virtual DbSet<OcrtextractRelationship> OcrtextractRelationships { get; set; }

    public virtual DbSet<OcrtextractWord> OcrtextractWords { get; set; }

    public virtual DbSet<RequestId> RequestIds { get; set; }

    public virtual DbSet<TextractBlock> TextractBlocks { get; set; }

    public virtual DbSet<Prompt1> Prompts => Set<Prompt1>();
    public virtual DbSet<ModelEndpoint> ModelEndpoints => Set<ModelEndpoint>();
    public virtual DbSet<WorktypeCatalog> WorktypeCatalogs => Set<WorktypeCatalog>();
    public virtual DbSet<PhraseOverride> PhraseOverrides => Set<PhraseOverride>();
    public virtual DbSet<CheckboxFlagRule> CheckboxFlagRules => Set<CheckboxFlagRule>();
    public virtual DbSet<DocumentJob> DocumentJobs => Set<DocumentJob>();
    public virtual DbSet<DocumentResult> DocumentResults => Set<DocumentResult>();

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer("Server=WLF1N1TAZSQLCLU.wlife.com.au;Database=IDPDEV;User Id=ARSUser;Password=hjytu73k48jk23!;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IDPAuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.ToTable("IDPAuditLog");
        });

        modelBuilder.Entity<Aillmresponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AILLMRes__3213E83FCD1D83B8");

            entity.ToTable("AILLMResponse");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PayloadToLlm)
                .IsUnicode(false)
                .HasColumnName("PayloadToLLM");
        });

        modelBuilder.Entity<BpmscaseEmail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPMSCase__3214EC07C530F69A");

            entity.ToTable("BPMSCaseEmails");

            entity.HasIndex(e => e.EmailId, "UQ__BPMSCase__7ED91ACE485CEBFA").IsUnique();

            entity.Property(e => e.BatchExternalId)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("BatchExternalID");
            entity.Property(e => e.BatchFolder)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.BatchName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.BpmscaseId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BPMSCaseId");
            entity.Property(e => e.BpmscaseStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BPMSCaseStatus");
            entity.Property(e => e.CaseStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CaseTarget)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CreateCase)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentSource)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EmailId)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EmailSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EmailSubject)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.IndexingSource)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDatetime).HasColumnType("datetime");
            entity.Property(e => e.SenderEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SenderName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SubWorkType)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TargetLocation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.WorkType)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BpmscaseMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPMSCase__3214EC07F1D6DC30");

            entity.ToTable("BPMSCaseMessages");

            entity.HasIndex(e => e.MessageId, "UQ__BPMSCase__C87C0C9DD107FD76").IsUnique();

            entity.Property(e => e.BatchExternalId)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("BatchExternalID");
            entity.Property(e => e.BatchFolder)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.BatchName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.BpmscaseId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BPMSCaseId");
            entity.Property(e => e.BpmscaseStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("BPMSCaseStatus");
            entity.Property(e => e.CaseStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CaseTarget)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CreateCase)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentSource)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IndexingSource)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ReceivedDatetime).HasColumnType("datetime");
            entity.Property(e => e.SenderName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SenderSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SourceAliasEmail)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SubWorkType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TargetLocation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.WorkType)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BpmscasesDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPMSCase__3214EC0721A06235");

            entity.ToTable("BPMSCasesDocuments");

            entity.Property(e => e.ArsdocumentId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ARSDocumentId");
            entity.Property(e => e.BpmscaseId).HasColumnName("BPMSCaseId");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.FilePath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.FileStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Bpmsconfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPMSConf__3214EC0788EB85F3");

            entity.ToTable("BPMSConfig");

            entity.Property(e => e.ConfigName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Key)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Value)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BpmspolicyNumber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PolicyNu__3214EC079118A07B");

            entity.ToTable("BPMSPolicyNumbers");

            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<BpmsrequestId>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BPMSRequ__3214EC079D999530");

            entity.ToTable("BPMSRequestIds");

            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.RequestId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<BpmsworkTypeMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__WorkType__3214EC07C9B1768F");

            entity.ToTable("BPMSWorkTypeMapping");

            entity.Property(e => e.DocType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.QueueCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SourceEmail)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SubWorkType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CmsasteronClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CMSAster__3214EC0721A2FC28");

            entity.ToTable("CMSAsteronClaims");

            entity.Property(e => e.ClaimNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ClaimsTeamDepartment)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CustomerFirstName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.CustomerLastName)
                .HasMaxLength(5250)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Postcode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CmscaseDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CMSCaseD__3214EC077466F66B");

            entity.ToTable("CMSCaseDocuments");

            entity.Property(e => e.ArsdocumentId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ARSDocumentId");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.FilePath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.FileStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<CmscaseUpdateMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CMSCaseU__3214EC07EA3EBDC5");

            entity.ToTable("CMSCaseUpdateMessages");

            entity.Property(e => e.BatchFolder)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CaseStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ClaimNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreateCase)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TargetLocation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Cmsconfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CMSConfi__3214EC07BBAB10F3");

            entity.ToTable("CMSConfig");

            entity.Property(e => e.ConfigName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Key)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Value)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CmsexceptionMailbox>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CMSExceptionMailbox");

            entity.Property(e => e.CaseSource)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CaseStatus)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CaseStatusMessage).IsUnicode(false);
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.ExceptionReason)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.Sno).ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<CmspostProcessingLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CMSPostProcessingLog");

            entity.Property(e => e.BatchFolder)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.ClaimNumber).IsUnicode(false);
            entity.Property(e => e.Cmsdoctype)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CMSDoctype");
            entity.Property(e => e.CreateCase)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FileType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MessageSource)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNumber).IsUnicode(false);
            entity.Property(e => e.Sno).ValueGeneratedOnAdd();
            entity.Property(e => e.TargetLocation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<ConvergaDocumentsQueue>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Converga__1ABEEF0F4A5C72A7");

            entity.ToTable("ConvergaDocumentsQueue");

            entity.Property(e => e.DocumentId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.DocumentSftpinterface)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DocumentSFTPInterface");
            entity.Property(e => e.DocumentSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProcessingStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeOfDocument)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<ConvergePhysicalDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Converge__1ABEEF0F09C4807F");

            entity.Property(e => e.DocumentId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.DocumentSftpinterface)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DocumentSFTPInterface");
            entity.Property(e => e.DocumentSource)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DocumentType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ProcessingStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeOfDocument)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MailClaimNumber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClaimNum__3214EC07553D20A6");

            entity.Property(e => e.ClaimNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<MailMessage1>(entity =>
        {
            entity.HasKey(e => e.MailMessageId);
            entity.ToTable("MailMessages");
        });

        modelBuilder.Entity<EntityModels.MailHeader>(entity =>
        {
            entity.HasKey(e => e.MailHeaderId);
            entity.ToTable("MailHeaders");
        });

        modelBuilder.Entity<EntityModels.MailRecipient>(entity =>
        {
            entity.HasKey(e => e.MailRecipientId);
            entity.ToTable("MailRecipients");
        });


        modelBuilder.Entity<EntityModels.MailAttachment>(entity =>
        {
            entity.HasKey(e => e.MailAttachmentId);
            entity.ToTable("MailAttachments");
        });

        modelBuilder.Entity<EntityModels.MailSyncState>(entity =>
        {
            entity.HasKey(e => e.MailSyncStateId);
            entity.ToTable("MailSyncStates");
        });

        modelBuilder.Entity<MailErrorOutput>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ErrorOut__3214EC07F837081D");

            entity.ToTable("MailErrorOutput");

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MailPolicyNumber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MailPoli__3214EC071891AA44");

            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<MailboxConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MailboxC__3214EC0782098414");

            entity.ToTable("MailboxConfig");

            entity.Property(e => e.ClientId)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.ClientSecret)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MailboxAddress)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TenantId)
                .HasMaxLength(500)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ocrdocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OCRDocum__3214EC0759437CE8");

            entity.ToTable("OCRDocuments");

            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.DocumentDpihorizontal).HasColumnName("DocumentDPIHorizontal");
            entity.Property(e => e.DocumentDpivertical).HasColumnName("DocumentDPIVertical");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.DocumentName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DocumentPath)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.DocumentPathType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.Ocrstatus)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("OCRStatus");
            entity.Property(e => e.OcrstatusMessage).HasColumnName("OCRStatusMessage");
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcrjsonForLlm>(entity =>
        {
            entity.HasKey(e => e.JsonId).HasName("PK__OCRJsonF__26D5DDE9D65B5955");

            entity.ToTable("OCRJsonForLLM");

            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.InputFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcroutputFromLlm>(entity =>
        {
            entity.HasKey(e => e.JsonId).HasName("PK__OCROutpu__26D5DDE96AD0DD20");

            entity.ToTable("OCROutputFromLLM");

            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.InputFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcrtextractBlock>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__OCRTextr__144215F1EF0AE05C");

            entity.ToTable("OCRTextractBlocks");

            entity.HasIndex(e => e.Id, "UQ__OCRTextr__3214EC06A4F0B452").IsUnique();

            entity.Property(e => e.BlockId).HasMaxLength(255);
            entity.Property(e => e.BlockType).HasMaxLength(50);
            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.InputFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TextType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Text_Type");
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcrtextractGeometry>(entity =>
        {
            entity.HasKey(e => e.GeometryId).HasName("PK__OCRTextr__449F4E8AC7D994F2");

            entity.ToTable("OCRTextractGeometry");

            entity.Property(e => e.BlockId).HasMaxLength(255);
            entity.Property(e => e.BoundingBoxHeight).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.BoundingBoxLeft).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.BoundingBoxTop).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.BoundingBoxWidth).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.InputFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcrtextractJsonDocument>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OCRTextractJsonDocument");

            entity.Property(e => e.JsonId).ValueGeneratedOnAdd();
            entity.Property(e => e.Ocrtype)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("OCRType");
        });

        modelBuilder.Entity<OcrtextractRelationship>(entity =>
        {
            entity.HasKey(e => e.RelationshipId).HasName("PK__OCRTextr__31FEB881B3FACD36");

            entity.ToTable("OCRTextractRelationships");

            entity.Property(e => e.BlockId).HasMaxLength(255);
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.InputFilePath)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RelatedBlockId).HasMaxLength(100);
            entity.Property(e => e.RelationType).HasMaxLength(50);
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<OcrtextractWord>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OCRTextract_Words");

            entity.Property(e => e.BlockType)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Confidence)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Id)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Ocrtext).HasColumnName("OCRText");
            entity.Property(e => e.OcrtextType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("OCRTextType");
            entity.Property(e => e.WordId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<RequestId>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestI__3214EC0758E6D00E");

            entity.Property(e => e.Confidence).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedDateTime).HasColumnType("datetime");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.MessageId)
                .HasMaxLength(1050)
                .IsUnicode(false);
            entity.Property(e => e.RequestId1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RequestId");
            entity.Property(e => e.UpdatedDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TextractBlock>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.BlockType).HasMaxLength(500);
            entity.Property(e => e.Confidence).HasMaxLength(500);
            entity.Property(e => e.Id).HasMaxLength(500);
            entity.Property(e => e.Text).HasMaxLength(500);
        });

        modelBuilder.Entity<Prompt1>(entity =>
        {
            entity.HasKey(e => e.PromptId);
            entity.ToTable("Prompts");
        });

        modelBuilder.Entity<ModelEndpoint>(entity =>
        {
            entity.HasKey(e => e.EndpointId);
            entity.ToTable("ModelEndpoints");
        });

        modelBuilder.Entity<WorktypeCatalog>(entity =>
        {
            entity.HasKey(e => e.CatalogId);
            entity.ToTable("WorktypeCatalogs");
        });
        modelBuilder.Entity<PhraseOverride>(entity =>
        {
            entity.HasKey(e => e.OverrideId);
            entity.ToTable("PhraseOverrides");
        });
        modelBuilder.Entity<CheckboxFlagRule>(entity =>
        {
            entity.HasKey(e => e.RuleId);
            entity.ToTable("CheckboxFlagRules");
        });

        modelBuilder.Entity<DocumentJob>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.ToTable("DocumentJobs");
        });

        modelBuilder.Entity<DocumentResult>(entity =>
        {
            entity.HasKey(e => e.JobId);
            entity.ToTable("DocumentResults");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

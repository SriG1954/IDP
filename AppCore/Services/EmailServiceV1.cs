using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Interfaces;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AppCore.Services;

public class EmailServiceV1 : IEmailSerivce
{
    private readonly AppDbContext _db;
    private readonly IIDPAuditLogRepository _audit;
    private readonly GraphServiceClient _graph;
    private readonly MailboxConfig _mailboxConfig;
    private readonly string _attachmentRoot;

    public EmailServiceV1(
        AppDbContext db,
        IConfiguration config,
        IIDPAuditLogRepository audit)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));

        try
        {
            _attachmentRoot = config["Mail:AttachmentRoot"]
                ?? throw new InvalidOperationException("Attachment root not configured.");

            _mailboxConfig = _db.MailboxConfigs
                .AsNoTracking()
                .FirstOrDefault()
                ?? throw new InvalidOperationException("Mailbox configuration missing.");

            var credential = new ClientSecretCredential(
                _mailboxConfig.TenantId!,
                _mailboxConfig.ClientId!,
                _mailboxConfig.ClientSecret!);

            _graph = new GraphServiceClient(credential);
        }
        catch (Exception ex)
        {
            _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage).GetAwaiter().GetResult();

            throw;
        }
    }

    // =========================================================
    // PUBLIC ENTRY POINT
    // =========================================================

    public async Task<int> ProcessInboxBatchForDateAsync(
        DateOnly date,
        int batchSize = 50,
        CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            ct.ThrowIfCancellationRequested();

            var mailbox = _mailboxConfig.MailboxAddress!;
            const string folderId = "inbox";

            var page = await FetchMessagesPageAsync(mailbox, folderId, date, batchSize, ct);

            if (page?.Value == null || page.Value.Count == 0)
            {
                await UpsertNextLinkAsync(mailbox, folderId, date, null, ct);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return 0;
            }

            foreach (var message in page.Value)
            {
                ct.ThrowIfCancellationRequested();

                var entity = await UpsertMailMessageAsync(mailbox, message, ct);

                ProcessHeaders(entity, message);
                ProcessRecipients(entity, message);

                if (message.HasAttachments == true)
                    await ProcessAttachmentsAsync(mailbox, message, entity, date, ct);
            }

            await _db.SaveChangesAsync(ct);

            await UpsertNextLinkAsync(mailbox, folderId, date, page.OdataNextLink, ct);
            await _db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return page.Value.Count;
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);

            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }

    public async Task<List<Message>> FetchInboxMessagesAsync(CancellationToken ct)
    {
        var mailbox = _mailboxConfig.MailboxAddress!;
        const string folderId = "inbox";

        try
        {
            var page = await FetchMessagesPageAsync(mailbox, folderId, DateOnly.FromDateTime(DateTime.UtcNow), 50, ct);
            return page?.Value?.ToList() ?? new List<Message>();
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);
            throw;
        }
    }

    // =========================================================
    // FETCH GRAPH PAGE
    // =========================================================

    private async Task<MessageCollectionResponse?> FetchMessagesPageAsync(
        string mailbox,
        string folderId,
        DateOnly date,
        int batchSize,
        CancellationToken ct)
    {
        try
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            var filter =
                $"receivedDateTime ge {start:yyyy-MM-ddTHH:mm:ssZ} and " +
                $"receivedDateTime lt {end:yyyy-MM-ddTHH:mm:ssZ}";

            return await _graph.Users[mailbox]
                .MailFolders[folderId]
                .Messages
                .GetAsync(rc =>
                {
                    rc.QueryParameters.Filter = filter;
                    rc.QueryParameters.Orderby = new[] { "receivedDateTime asc" };
                    rc.QueryParameters.Top = batchSize;
                    rc.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                }, ct);
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }

    // =========================================================
    // UPSERT MESSAGE
    // =========================================================

    private async Task<MailMessage1> UpsertMailMessageAsync(
        string mailbox,
        Message m,
        CancellationToken ct)
    {
        try
        {
            MailMessage1? entity = null;

            if (!string.IsNullOrWhiteSpace(m.InternetMessageId))
            {
                entity = await _db.MailMessages
                    .FirstOrDefaultAsync(x =>
                        x.Mailbox == mailbox &&
                        x.InternetMessageId == m.InternetMessageId, ct);
            }

            entity ??= await _db.MailMessages
                .FirstOrDefaultAsync(x =>
                    x.Mailbox == mailbox &&
                    x.GraphMessageId == m.Id, ct);

            if (entity == null)
            {
                entity = new MailMessage1
                {
                    Mailbox = mailbox,
                    GraphMessageId = m.Id!,
                    InternetMessageId = m.InternetMessageId,
                    Subject = m.Subject,
                    FromAddress = m.From?.EmailAddress?.Address,
                    ReceivedDateTimeUtc = m.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                _db.MailMessages.Add(entity);
            }
            else
            {
                entity.Subject = m.Subject;
                entity.FromAddress = m.From?.EmailAddress?.Address;
                entity.UpdatedAtUtc = DateTime.UtcNow;
            }

            return entity;
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }

    // =========================================================
    // HEADERS
    // =========================================================

    private void ProcessHeaders(MailMessage1 entity, Message m)
    {
        try
        {
            if (m.InternetMessageHeaders == null) return;

            foreach (var header in m.InternetMessageHeaders
                         .Where(h => !string.IsNullOrWhiteSpace(h?.Name)))
            {
                _db.MailHeaders.Add(new MailHeader
                {
                    Name = header!.Name!,
                    Value = header.Value
                });
            }
        }
        catch (Exception ex)
        {
            _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage).GetAwaiter().GetResult();

            throw;
        }
    }

    // =========================================================
    // RECIPIENTS
    // =========================================================

    private void ProcessRecipients(MailMessage1 entity, Message m)
    {
        try
        {
            if (m.ToRecipients != null)
            {
                foreach (var r in m.ToRecipients)
                {
                    _db.MailRecipients.Add(new MailRecipient
                    {
                        Type = "To",
                        EmailAddress = r.EmailAddress?.Address!,
                        DisplayName = r.EmailAddress?.Name
                    });
                }
            }

            if (m.CcRecipients != null)
            {
                foreach (var r in m.CcRecipients)
                {
                    _db.MailRecipients.Add(new MailRecipient
                    {
                        Type = "Cc",
                        EmailAddress = r.EmailAddress?.Address!,
                        DisplayName = r.EmailAddress?.Name
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage).GetAwaiter().GetResult();

            throw;
        }
    }

    // =========================================================
    // ATTACHMENTS
    // =========================================================

    private async Task ProcessAttachmentsAsync(
        string mailbox,
        Message m,
        MailMessage1 msg,
        DateOnly date,
        CancellationToken ct)
    {
        try
        {
            var atts = await _graph.Users[mailbox]
                .Messages[m.Id]
                .Attachments
                .GetAsync();

            if (atts?.Value == null) return;

            foreach (var att in atts.Value.OfType<FileAttachment>())
            {
                if (_db.MailAttachments.Any(x => x.GraphAttachmentId == att.Id))
                    continue;

                Task<Stream> factory() =>
                    DownloadAttachmentStreamAsync(mailbox, m.Id!, att.Id!, ct);

                var (path, hash, size) =
                    await AttachmentIO.SaveAsync(
                        factory,
                        _attachmentRoot,
                        date,
                        m.Id!,
                        att.Name ?? "attachment",
                        ct);

                _db.MailAttachments.Add(new MailAttachment
                {
                    GraphAttachmentId = att.Id!,
                    Name = att.Name,
                    SizeBytes = size,
                    Sha256Hex = hash,
                    SavedPath = path,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }

    // =========================================================
    // NEXT LINK
    // =========================================================

    private async Task UpsertNextLinkAsync(
        string mailbox,
        string folderId,
        DateOnly date,
        string? nextLink,
        CancellationToken ct)
    {
        try
        {
            var day = date.ToDateTime(TimeOnly.MinValue);

            var state = await _db.MailSyncStates
                .SingleOrDefaultAsync(x =>
                    x.Mailbox == mailbox &&
                    x.FolderId == folderId &&
                    x.DateUtc == day, ct);

            if (state == null)
            {
                _db.MailSyncStates.Add(new MailSyncState
                {
                    Mailbox = mailbox,
                    FolderId = folderId,
                    DateUtc = day,
                    OdataNextLink = nextLink,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                state.OdataNextLink = nextLink;
                state.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }

    // =========================================================
    // DOWNLOAD STREAM
    // =========================================================

    private async Task<Stream> DownloadAttachmentStreamAsync(
        string mailbox,
        string messageId,
        string attachmentId,
        CancellationToken ct)
    {
        try
        {
            var request = _graph.Users[mailbox]
                .Messages[messageId]
                .Attachments[attachmentId]
                .ToGetRequestInformation();

            request.UrlTemplate = request.UrlTemplate!
                .Insert(request.UrlTemplate.LastIndexOf('{'), "/$value");

            var stream = await _graph.RequestAdapter
                .SendPrimitiveAsync<Stream>(request, cancellationToken: ct);

            return stream ?? Stream.Null;
        }
        catch (Exception ex)
        {
            await _audit.AddLogAsync(0, 1, ex.ToString(),
                AuditLogLevel.Error,
                AuditEventType.FetchEmailMessage);

            throw;
        }
    }
}
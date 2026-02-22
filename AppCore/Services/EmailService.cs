using Amazon.BedrockRuntime.Model;
using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Interfaces;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AppCore.Services
{
    public class EmailService : IEmailSerivce
    {
        private readonly AppDbContext _db;
        private readonly IIDPAuditLogRepository _audit;
        private readonly GraphServiceClient _graph;
        private readonly IConfiguration _cfg;
        private string _sourceEmailAddress = string.Empty;
        private string _baseAttachmentFolder = @"C:\Temp\MailAttachments";

        public EmailService(AppDbContext db, IConfiguration cfg, IIDPAuditLogRepository audit)
        {
            _db = db;
            _cfg = cfg;
            _audit = audit;

            var tenantId = _cfg["EmailClientCredentials:TenantId"]!;
            var clientId = _cfg["EmailClientCredentials:ClientId"]!;
            var clientSecret = "V6Q8Q~hM3rc7ku7j9PGuBaB2ll6Fq0tTLPwgkaqr"; //_cfg["EmailClientCredentials:ClientHide"]!;
            _sourceEmailAddress = _cfg["EmailUserCredentials:Username"]!;

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _graph = new GraphServiceClient(credential);
        }

        public async Task<List<Microsoft.Graph.Models.Message>> FetchInboxMessagesAsync(CancellationToken ct)
        {
            try
            {
                var messages = await _graph.Users[_sourceEmailAddress].MailFolders["inbox"].Messages
                       .GetAsync(requestConfig =>
                       {
                           requestConfig.QueryParameters.Orderby = new[] { "receivedDateTime asc" };
                           requestConfig.QueryParameters.Top = 10;
                           requestConfig.QueryParameters.Select = new[] {
                            "id",
                            "subject",
                            "from",
                            "receivedDateTime",
                            "toRecipients",
                            "ccRecipients",
                            "body",
                            "hasAttachments",
                            "internetMessageHeaders", // Email Header
                            "sender", // Sender Information
                            "importance", // importance header
                            "internetMessageId" // message Id
                           };
                       });

                return messages?.Value ?? new List<Microsoft.Graph.Models.Message>();
            }
            catch (Exception ex)
            {
                await _audit.AddLogAsync(0, 1, ex.ToString(), AuditLogLevel.Error, AuditEventType.FetchEmailMessage);
                throw;
            }
        }

        public async Task<int> ProcessInboxBatchForDateAsync(string mailbox, DateOnly date, int batchSize = 25, CancellationToken ct = default)
        {
            const string folderId = "inbox";

            // Load persisted nextLink (if any)
            var state = await _db.MailSyncStates
                .SingleOrDefaultAsync(x => x.Mailbox == mailbox && x.FolderId == folderId && x.DateUtc == date.ToDateTime(TimeOnly.MinValue), ct);

            MessageCollectionResponse? page;

            if (!string.IsNullOrEmpty(state?.OdataNextLink))
            {
                // Resume where we left
                page = await _graph.Users[mailbox].MailFolders[folderId].Messages.WithUrl(state.OdataNextLink).GetAsync(null, ct);
            }
            else
            {
                var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var end = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                var filter = $"receivedDateTime ge {start:yyyy-MM-ddTHH:mm:ssZ} and receivedDateTime lt {end:yyyy-MM-ddTHH:mm:ssZ}";
                page = await _graph.Users[mailbox].MailFolders[folderId].Messages.GetAsync(rc =>
                {
                    rc.QueryParameters.Filter = filter;
                    rc.QueryParameters.Orderby = new[] { "receivedDateTime asc" };
                    rc.QueryParameters.Top = batchSize;
                    rc.QueryParameters.Select = new[]
                    {
                    "id","subject","from","sender","receivedDateTime",
                    "toRecipients","ccRecipients",
                    "bodyPreview","hasAttachments",
                    "internetMessageHeaders","importance","internetMessageId","conversationId"
                };
                    rc.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                }, ct);
            }

            if (page?.Value == null || page.Value.Count == 0)
            {
                await UpsertNextLinkAsync(mailbox, folderId, date, null, ct);
                return 0;
            }

            int processed = 0;

            foreach (var m in page.Value)
            {
                try
                {
                    var msgRow = await UpsertMailMessageAsync(mailbox, m, ct);

                    // Headers
                    if (m.InternetMessageHeaders is not null)
                    {
                        foreach (var h in m.InternetMessageHeaders.Where(h => !string.IsNullOrWhiteSpace(h?.Name)))
                        {
                            await UpsertHeaderAsync(msgRow, h!.Name!, h.Value, ct);
                        }
                    }

                    // Recipients (To, Cc)
                    if (m.ToRecipients is not null)
                        foreach (var r in m.ToRecipients)
                            await UpsertRecipientAsync(msgRow, "To", r, ct);

                    if (m.CcRecipients is not null)
                        foreach (var r in m.CcRecipients)
                            await UpsertRecipientAsync(msgRow, "Cc", r, ct);

                    // Attachments
                    if (m.HasAttachments == true)
                        await ProcessAttachmentsAsync(mailbox, m, msgRow, date, ct);

                    processed++;
                }
                catch (DbUpdateException ex)
                {
                    await _audit.AddLogAsync(0, 0, $"Error: {ex.ToString()} Concurrency/unique violation for message {m.Id} - skipping duplicate", AuditLogLevel.Error, AuditEventType.FetchEmailMessage);
                    throw;
                }
                catch (Exception ex)
                {
                    await _audit.AddLogAsync(0, 0, $"Error: {ex.ToString()} Failed processing message {m.Id}", AuditLogLevel.Error, AuditEventType.FetchEmailMessage);
                    throw;
                }
            }

            // Save once per batch (changes are tracked per entity)
            await _db.SaveChangesAsync(ct);

            // Persist nextLink for the next run
            await UpsertNextLinkAsync(mailbox, folderId, date, page.OdataNextLink, ct);

            return processed;
        }

        private async Task<EntityModels.MailMessage> UpsertMailMessageAsync(string mailbox, Microsoft.Graph.Models.Message m, CancellationToken ct)
        {
            // Try by InternetMessageId (stable) then GraphMessageId
            EntityModels.MailMessage? entity = null;

            if (!string.IsNullOrEmpty(m.InternetMessageId))
            {
                entity = await _db.MailMessages.FirstOrDefaultAsync(x =>
                    x.Mailbox == mailbox && x.InternetMessageId == m.InternetMessageId, ct);
            }

            entity ??= await _db.MailMessages.FirstOrDefaultAsync(x =>
                x.Mailbox == mailbox && x.GraphMessageId == m.Id, ct);

            if (entity is null)
            {
                entity = new EntityModels.MailMessage
                {
                    Mailbox = mailbox,
                    GraphMessageId = m.Id!,
                    InternetMessageId = m.InternetMessageId,
                    Subject = m.Subject,
                    FromAddress = m.From?.EmailAddress?.Address,
                    SenderAddress = m.Sender?.EmailAddress?.Address,
                    ReceivedDateTimeUtc = m.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    Importance = m.Importance?.ToString(),
                    HasAttachments = m.HasAttachments ?? false,
                    ConversationId = m.ConversationId,
                    BodyPreview = m.BodyPreview
                };
                _db.MailMessages.Add(entity);
            }
            else
            {
                entity.GraphMessageId = m.Id!;
                entity.InternetMessageId = m.InternetMessageId ?? entity.InternetMessageId;
                entity.Subject = m.Subject;
                entity.FromAddress = m.From?.EmailAddress?.Address;
                entity.SenderAddress = m.Sender?.EmailAddress?.Address;
                entity.ReceivedDateTimeUtc = m.ReceivedDateTime?.UtcDateTime ?? entity.ReceivedDateTimeUtc;
                entity.Importance = m.Importance?.ToString();
                entity.HasAttachments = m.HasAttachments ?? entity.HasAttachments;
                entity.ConversationId = m.ConversationId ?? entity.ConversationId;
                entity.BodyPreview = m.BodyPreview ?? entity.BodyPreview;

                _db.MailMessages.Update(entity);
            }

            // Save now to get MailMessageId for FKs (keeps logic simple)
            await _db.SaveChangesAsync(ct);
            return entity;
        }

        private async Task UpsertHeaderAsync(EntityModels.MailMessage msg, string name, string? value, CancellationToken ct)
        {
            var existing = await _db.MailHeaders
                .FirstOrDefaultAsync(x => x.MailMessageId == msg.MailMessageId && x.Name == name, ct);

            if (existing is null)
            {
                _db.MailHeaders.Add(new MailHeader
                {
                    MailMessageId = msg.MailMessageId,
                    Name = name,
                    Value = value
                });
            }
            else
            {
                existing.Value = value;
                _db.MailHeaders.Update(existing);
            }
        }

        private async Task UpsertRecipientAsync(EntityModels.MailMessage msg, string type, Recipient r, CancellationToken ct)
        {
            var email = r.EmailAddress?.Address ?? string.Empty;
            var display = r.EmailAddress?.Name;

            var existing = await _db.MailRecipients.FirstOrDefaultAsync(x =>
                x.MailMessageId == msg.MailMessageId && x.Type == type && x.EmailAddress == email, ct);

            if (existing is null)
            {
                _db.MailRecipients.Add(new MailRecipient
                {
                    MailMessageId = msg.MailMessageId,
                    Type = type,
                    DisplayName = display,
                    EmailAddress = email
                });
            }
            else
            {
                existing.DisplayName = display;
                _db.MailRecipients.Update(existing);
            }
        }

        private async Task ProcessAttachmentsAsync(string mailbox, Microsoft.Graph.Models.Message m, EntityModels.MailMessage msg, DateOnly date, CancellationToken ct)
        {
            var atts = await _graph.Users[mailbox].Messages[m.Id].Attachments.GetAsync(rc =>
            {
                rc.QueryParameters.Select = new[] { "id", "name", "size", "contentType", "isInline", "@odata.type" };
            }, ct);

            if (atts?.Value == null) return;

            foreach (var att in atts.Value)
            {
                if (att is not FileAttachment fa) continue; // extend for ItemAttachment/ReferenceAttachment if needed

                // Check if already persisted
                var exists = await _db.MailAttachments.AnyAsync(x =>
                    x.MailMessageId == msg.MailMessageId && x.GraphAttachmentId == fa.Id, ct);
                if (exists) continue;

                // Prefer streaming via /$value to avoid large memory usage
                Task<Stream> ContentFactory() => DownloadAttachmentStreamAsync(mailbox, m.Id!, fa.Id!, ct);

                var (savedPath, sha256Hex, size) = await AttachmentIO.SaveAsync(
                    ContentFactory, _baseAttachmentFolder, date, m.Id!, fa.Name ?? "attachment", ct);

                var row = new MailAttachment
                {
                    MailMessageId = msg.MailMessageId,
                    GraphAttachmentId = fa.Id!,
                    Name = fa.Name,
                    ContentType = fa.ContentType,
                    SizeBytes = size,
                    IsInline = fa.IsInline ?? false,
                    SavedPath = savedPath,
                    Sha256Hex = sha256Hex
                };

                _db.MailAttachments.Add(row);
            }
        }

        private async Task UpsertNextLinkAsync(string mailbox, string folderId, DateOnly date, string? nextLink, CancellationToken ct)
        {
            var day = date.ToDateTime(TimeOnly.MinValue);
            var state = await _db.MailSyncStates.SingleOrDefaultAsync(x =>
                x.Mailbox == mailbox && x.FolderId == folderId && x.DateUtc == day, ct);

            if (state is null)
            {
                state = new MailSyncState
                {
                    Mailbox = mailbox,
                    FolderId = folderId,
                    DateUtc = day,
                    OdataNextLink = nextLink,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _db.MailSyncStates.Add(state);
            }
            else
            {
                state.OdataNextLink = nextLink;
                state.UpdatedAtUtc = DateTime.UtcNow;
                _db.MailSyncStates.Update(state);
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task<Stream> DownloadAttachmentStreamAsync(string mailbox, string messageId, string attachmentId, CancellationToken ct)
        {
            var reqInfo = _graph.Users[mailbox].Messages[messageId].Attachments[attachmentId].ToGetRequestInformation();
            reqInfo.UrlTemplate = reqInfo.UrlTemplate!.Insert(reqInfo.UrlTemplate.LastIndexOf('{'), "/$value");
            var stream = await _graph.RequestAdapter.SendPrimitiveAsync<Stream>(reqInfo, cancellationToken: ct);
            return stream ?? Stream.Null;
        }

    }
}

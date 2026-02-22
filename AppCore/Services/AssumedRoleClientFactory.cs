using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Textract;
using AppCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AppCore.Services
{
    public class AssumedRoleClientFactory : IAssumedRoleClientFactory, IDisposable
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<AssumedRoleClientFactory> _logger;
        private readonly IIDPAuditLogRepository _audit;
        private readonly SemaphoreSlim _assumeLock = new(1, 1);
        private S3ClientDescriptor? _s3cached;
        private TextractClientDescriptor? _textractCached;
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(2);

        public AssumedRoleClientFactory(IConfiguration cfg, ILogger<AssumedRoleClientFactory> logger, IIDPAuditLogRepository audit)
        {
            _cfg = cfg;
            _logger = logger;
            _audit = audit;
        }

        public async Task<S3ClientDescriptor> CreateS3ClientAsync(CancellationToken ct = default)
        {
            var environment = _cfg["AWS:Environment"];
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            if (_s3cached is not null && DateTime.UtcNow < _s3cached.ExpiryUtc - RefreshBuffer)
                return _s3cached;

            await _assumeLock.WaitAsync(ct);
            try
            {
                if (_s3cached is not null && DateTime.UtcNow < _s3cached.ExpiryUtc - RefreshBuffer)
                    return _s3cached;

                await SafeAuditAsync("AssumeRole: starting", AuditLogLevel.Info);

                var region = RegionEndpoint.APSoutheast2;
                var stsConfig = new AmazonSecurityTokenServiceConfig { RegionEndpoint = region };

                var proxyHost = _cfg["AWS:ProxyHost"];
                if (!string.IsNullOrEmpty(proxyHost) && int.TryParse(_cfg["AWS:ProxyPort"], out var p))
                {
                    stsConfig.ProxyHost = proxyHost;
                    stsConfig.ProxyPort = p;
                }

                using var sts = new AmazonSecurityTokenServiceClient(new InstanceProfileAWSCredentials(), stsConfig);

                var assumeReq = new AssumeRoleRequest
                {
                    RoleArn = _cfg["AWS:RoleArn"],
                    RoleSessionName = _cfg["AWS:RoleSessionName"] ?? "AppSession",
                    DurationSeconds = int.TryParse(_cfg["AWS:RoleDurationSeconds"], out var d) ? d : 3600
                };

                var resp = await sts.AssumeRoleAsync(assumeReq, ct);

                await SafeAuditAsync("Received AssumeRoleResponse resp", AuditLogLevel.Info);

                var creds = new SessionAWSCredentials(resp.Credentials.AccessKeyId, resp.Credentials.SecretAccessKey, resp.Credentials.SessionToken);

                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = region,
                    ThrottleRetries = true,
                    RetryMode = RequestRetryMode.Adaptive
                };

                await SafeAuditAsync("Creating AmazonS3Client", AuditLogLevel.Info);

                var s3 = new AmazonS3Client(creds, s3Config);
                var expiry = DateTime.SpecifyKind(resp.Credentials.Expiration!.Value, DateTimeKind.Utc);

                _s3cached = new S3ClientDescriptor(s3, expiry);

                await SafeAuditAsync($"AssumeRole: success; expires {expiry:u}", AuditLogLevel.Info);

                return _s3cached;
            }
            catch (Exception ex)
            {
                await SafeAuditAsync($"AssumeRole: failed {ex.Message}", AuditLogLevel.Error);
                throw;
            }
            finally
            {
                _assumeLock.Release();
            }
        }


        public async Task<TextractClientDescriptor> CreateTextractClientAsync(CancellationToken ct = default)
        {
            var environment = _cfg["AWS:Environment"];
            if (environment == null)
                throw new ArgumentNullException(nameof(environment));

            // Fast-path: return cached instance if still valid
            if (_textractCached is not null && DateTime.UtcNow < _textractCached.ExpiryUtc - RefreshBuffer)
                return _textractCached;

            await _assumeLock.WaitAsync(ct);
            try
            {
                if (_textractCached is not null && DateTime.UtcNow < _textractCached.ExpiryUtc - RefreshBuffer)
                    return _textractCached;

                await SafeAuditAsync("AssumeRole (Textract): starting", AuditLogLevel.Info);

                var region = RegionEndpoint.APSoutheast2;
                var stsConfig = new AmazonSecurityTokenServiceConfig { RegionEndpoint = region };

                var proxyHost = _cfg["AWS:ProxyHost"];
                if (!string.IsNullOrEmpty(proxyHost) && int.TryParse(_cfg["AWS:ProxyPort"], out var p))
                {
                    stsConfig.ProxyHost = proxyHost;
                    stsConfig.ProxyPort = p;
                }

                using var sts = new AmazonSecurityTokenServiceClient(new InstanceProfileAWSCredentials(), stsConfig);

                var assumeReq = new AssumeRoleRequest
                {
                    RoleArn = _cfg["AWS:RoleArn"],
                    RoleSessionName = _cfg["AWS:RoleSessionName"] ?? "AppSession",
                    DurationSeconds = int.TryParse(_cfg["AWS:RoleDurationSeconds"], out var d) ? d : 3600
                };

                var resp = await sts.AssumeRoleAsync(assumeReq, ct);

                await SafeAuditAsync("Received AssumeRoleResponse resp", AuditLogLevel.Info);

                var creds = new SessionAWSCredentials(resp.Credentials.AccessKeyId, resp.Credentials.SecretAccessKey, resp.Credentials.SessionToken);

                var textractConfig = new AmazonTextractConfig
                {
                    RegionEndpoint = region,
                    ThrottleRetries = true,
                    RetryMode = RequestRetryMode.Adaptive
                };

                await SafeAuditAsync("Creating AmazonTextractClient", AuditLogLevel.Info);

                var textract = new AmazonTextractClient(creds, textractConfig);
                var expiry = DateTime.SpecifyKind(resp.Credentials.Expiration!.Value, DateTimeKind.Utc);

                _textractCached = new TextractClientDescriptor(textract, expiry);

                await SafeAuditAsync($"AssumeRole (Textract): success; expires {expiry:u}", AuditLogLevel.Info);

                return _textractCached;
            }
            catch (Exception ex)
            {
                await SafeAuditAsync($"AssumeRole (Textract): failed {ex.Message}", AuditLogLevel.Error);
                throw;
            }
            finally
            {
                _assumeLock.Release();
            }
        }

        private async Task SafeAuditAsync(string message, AuditLogLevel level)
        {
            try
            {
                await _audit.AddLogAsync(0, 0, message, level, AuditEventType.AWSContext);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit write failed");
            }
        }

        public void Dispose()
        {
            _s3cached?.Client?.Dispose();
            _assumeLock?.Dispose();
        }
    }

}

using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.SageMakerRuntime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Textract;
using AppCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace AppCore.Services
{
    public class AssumedRoleClientFactory : IAssumedRoleClientFactory, IDisposable
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<AssumedRoleClientFactory> _logger;
        private readonly IIDPAuditLogRepository _audit;
        private readonly SemaphoreSlim _assumeLock = new(1, 1);
        private IAmazonS3? amazonS3 = null;
        private IAmazonTextract? amazonTextract = null;
        private IAmazonSageMakerRuntime? amazonSageMakerRuntime = null;
        private static readonly TimeSpan RefreshBuffer = TimeSpan.FromMinutes(2);

        public AssumedRoleClientFactory(IConfiguration cfg, ILogger<AssumedRoleClientFactory> logger, IIDPAuditLogRepository audit)
        {
            _cfg = cfg;
            _logger = logger;
            _audit = audit;
        }

        public async Task<IAmazonTextract> GetTextractClientAsync(CancellationToken ct = default)
        {
            string ProfileName = _cfg["AWSDTS:ProfileName"] ?? throw new InvalidOperationException("AWS:ProfileName is required");

            try
            {
                var chain = new CredentialProfileStoreChain();

                if (!chain.TryGetAWSCredentials(ProfileName, out var credentials))
                    throw new InvalidOperationException($"AWS profile '{ProfileName}' not found.");

                amazonTextract = new AmazonTextractClient(
                    credentials,
                    RegionEndpoint.APSoutheast2);

                if (amazonTextract == null)
                {
                    throw new InvalidOperationException("Unable to create Textract client.");
                }

                return (IAmazonTextract)amazonTextract;

            }
            catch (OperationCanceledException)
            {
                await SafeAuditAsync("GetTextractClientAsync cancelled", AuditLogLevel.Error);
                throw;
            }
            catch (InvalidOperationException)
            {
                await SafeAuditAsync("GetTextractClientAsync error", AuditLogLevel.Error);
                throw;
            }

            catch (Exception ex)
            {
                await SafeAuditAsync($"GetTextractClientAsync Exception error: {ex.Message}", AuditLogLevel.Error);
                throw;
            }
        }

        public async Task<AmazonSageMakerRuntimeClient> GetAmazonSageMakerRuntimeClientAsync(CancellationToken ct = default)
        {
            string ProfileName = _cfg["AWSDTS:ProfileName"] ?? throw new InvalidOperationException("AWS:ProfileName is required");

            try
            {
                var chain = new CredentialProfileStoreChain();

                if (!chain.TryGetAWSCredentials(ProfileName, out var credentials))
                    throw new InvalidOperationException($"AWS profile '{ProfileName}' not found.");

                amazonSageMakerRuntime = new AmazonSageMakerRuntimeClient(
                    credentials,
                    RegionEndpoint.APSoutheast2);

                if (amazonSageMakerRuntime == null)
                {
                    throw new InvalidOperationException("Unable to create AmazonSageMakerRuntimeClient.");
                }

                return (AmazonSageMakerRuntimeClient)amazonSageMakerRuntime;
            }

            catch (OperationCanceledException)
            {
                await SafeAuditAsync("GetAmazonSageMakerRuntimeClientAsync cancelled", AuditLogLevel.Error);
                throw;
            }
            catch (InvalidOperationException)
            {
                await SafeAuditAsync("GetAmazonSageMakerRuntimeClientAsync error", AuditLogLevel.Error);
                throw;
            }

            catch (Exception ex)
            {
                await SafeAuditAsync($"GetAmazonSageMakerRuntimeClientAsync Exception error: {ex.Message}", AuditLogLevel.Error);
                throw;
            }
        }

        public async Task<IAmazonS3> GetS3ClientAsync(CancellationToken ct = default)
        {
            string ProfileName = _cfg["AWSDTS:ProfileName"] ?? throw new InvalidOperationException("AWS:ProfileName is required");

            try
            {
                var chain = new CredentialProfileStoreChain();

                if (!chain.TryGetAWSCredentials(ProfileName, out var credentials))
                    throw new InvalidOperationException($"AWS profile '{ProfileName}' not found.");

                amazonS3 = new AmazonS3Client(
                    credentials,
                    RegionEndpoint.APSoutheast2);

                if (amazonS3 == null)
                {
                    throw new InvalidOperationException("Unable to create AmazonS3Client.");
                }

                return (IAmazonS3)amazonS3;
            }

            catch (OperationCanceledException)
            {
                await SafeAuditAsync("GetS3ClientAsync cancelled", AuditLogLevel.Error);
                throw;
            }
            catch (InvalidOperationException)
            {
                await SafeAuditAsync("GetS3ClientAsync InvalidOperationException error", AuditLogLevel.Error);
                throw;
            }

            catch (Exception ex)
            {
                await SafeAuditAsync($"GetS3ClientAsync Exception error: {ex.Message}", AuditLogLevel.Error);
                throw;
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
            _assumeLock?.Dispose();
        }
    }

}

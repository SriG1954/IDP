using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SageMakerRuntime;
using Amazon.SageMakerRuntime.Model;
using AppCore.EntityModels;
using AppCore.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppCore.Services
{
    public class VLMService : IVLMService
    {
        private readonly IConfiguration _cfg;
        private readonly IIDPAuditLogRepository _audit;
        private readonly IAssumedRoleClientFactory _factory;
        private readonly ILogger<VLMService> _logger;
        private readonly VlmOptions _options;
        //private IAmazonSageMakerRuntime? _runtime;

        public VLMService(IConfiguration cfg, 
            IIDPAuditLogRepository audit,
            IAssumedRoleClientFactory factory,
            IOptions<VlmOptions> options,
            ILogger<VLMService> logger)
        {
            _cfg = cfg;
            _audit = audit;
            _factory = factory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<string> CallKvAsync(
            string endpointName,
            string kvPrompt,
            Dictionary<string, object> textractLines,
            string imagePath,
            int maxTokens,
            double temperature,
            CancellationToken ct)
        {
            ValidateImage(imagePath);

            var payload = BuildPayload(
                kvPrompt + "\n\nOCR Data:\n" + JsonSerializer.Serialize(textractLines),
                imagePath,
                maxTokens,
                temperature);

            return await InvokeAsync(endpointName, payload, ct);
        }

        public async Task<string> CallClassifierAsync(
          string endpointName,
          string kvSummary,
          string workTypeCatalog,
          string imagePath,
          string classifierInstructions,
          int maxTokens,
          double temperature,
          CancellationToken ct)
        {
            ValidateImage(imagePath);
            var catalogJson = JsonSerializer.Serialize(workTypeCatalog,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                });

            var inputJson = JsonSerializer.Serialize(kvSummary,
              new JsonSerializerOptions
              {
                  WriteIndented = true,
              });

            var textPrompt = $"""
                {classifierInstructions}

                CATALOG:
                {catalogJson}

                INPUT:
                {inputJson}
                """;

            var payload = BuildPayload(textPrompt, imagePath, maxTokens, temperature);

            return await InvokeAsync(endpointName, payload, ct);
        }

        private object BuildPayload(string text, string imagePath, int maxTokens, double temperature)
        {
            var b64 = Convert.ToBase64String(File.ReadAllBytes(imagePath));
            var mime = GetMime(imagePath);

            return new
            {
                messages = new[]
                {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{b64}" } },
                        new { type = "text", text }
                    }
                }
            },
                max_tokens = maxTokens,
                temperature
            };
        }

        private async Task<string> InvokeAsync(string endpointName, object payload, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(payload);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            var request = new InvokeEndpointRequest
            {
                EndpointName = endpointName,
                ContentType = "application/json",
                Body = stream
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var _runtime = await _factory.GetAmazonSageMakerRuntimeClientAsync(ct).ConfigureAwait(false);

                var response = await _runtime.InvokeEndpointAsync(request, ct);
                stopwatch.Stop();

                using var reader = new StreamReader(response.Body);
                var raw = await reader.ReadToEndAsync();

                _logger.LogInformation(
                    "SageMaker call success. Endpoint: {Endpoint}, DurationMs: {Duration}",
                    endpointName, stopwatch.ElapsedMilliseconds);

                return ExtractContentSafely(raw);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "SageMaker call failed. Endpoint: {Endpoint}, DurationMs: {Duration}",
                    endpointName, stopwatch.ElapsedMilliseconds);

                await SafeAuditAsync(
                    $"SageMaker error: {ex.Message}",
                    AuditLogLevel.Error,
                    AuditEventType.AWSContext);

                throw;
            }
        }

        private static string ExtractContentSafely(string raw)
        {
            using var doc = JsonDocument.Parse(raw);

            if (!doc.RootElement.TryGetProperty("choices", out var choices))
                throw new InvalidOperationException("Invalid model response: missing 'choices'");

            var content = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? string.Empty;
        }

        private void ValidateImage(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Image not found", path);

            var fileInfo = new FileInfo(path);

            if (fileInfo.Length > _options.MaxImageSizeMb * 1024 * 1024)
                throw new InvalidOperationException("Image exceeds size limit.");
        }

        private static string GetMime(string path)
            => Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpeg" or ".jpg" => "image/jpeg",
                _ => "image/jpeg"
            };


        private static Dictionary<string, object>? TryParseJson(string raw)
        {
            try 
            { 
                return JsonSerializer.Deserialize<Dictionary<string, object>>(raw); 
            }
            catch 
            { 
                return null; 
            }
        }

        private async Task SafeAuditAsync(string message, AuditLogLevel level, AuditEventType evtType)
        {
            try
            {
                await _audit.AddLogAsync(0, 0, message, level, evtType);
            }
            catch { }
        }
    }
}

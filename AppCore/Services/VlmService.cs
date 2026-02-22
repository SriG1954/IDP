using AppCore.Interfaces;
using System.Text;
using System.Text.Json;
using Amazon.SageMakerRuntime;
using Amazon.SageMakerRuntime.Model;

namespace AppCore.Services
{
    public sealed class VlmService : IVlmService
    {
        private readonly IAmazonSageMakerRuntime _rt;

        public VlmService(IAmazonSageMakerRuntime rt) => _rt = rt;

        private static string MimeFor(string path)
            => Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                _ => "image/jpeg"
            };

        private static string EncodeImage(string path)
            => Convert.ToBase64String(File.ReadAllBytes(path));

        private async Task<string> InvokeAsync(string endpointName, object payload, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(payload);
            var response = await _rt.InvokeEndpointAsync(new InvokeEndpointRequest
            {
                EndpointName = endpointName,
                ContentType = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(json))
            }, ct);

            using var sr = new StreamReader(response.Body);
            return await sr.ReadToEndAsync();
        }

        public async Task<string> CallKvAsync(string endpointName, string kvPrompt, Dictionary<string, object> textractLines,
            string imagePath, int maxTokens, double temperature, CancellationToken ct)
        {
            var b64 = EncodeImage(imagePath);
            var mime = MimeFor(imagePath);
            var payload = new
            {
                messages = new object[]
                {
                new {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{b64}" } },
                        new { type = "text", text = kvPrompt + "\n\nOCR Data:\n" + JsonSerializer.Serialize(textractLines) }
                    }
                }
                },
                max_tokens = maxTokens,
                temperature = temperature
            };

            var raw = await InvokeAsync(endpointName, payload, ct);
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
        }

        public async Task<string> CallClassifierAsync(string endpointName, string promptWithImage, string imagePath,
            int maxTokens, double temperature, CancellationToken ct)
        {
            var b64 = EncodeImage(imagePath);
            var mime = MimeFor(imagePath);

            var payload = new
            {
                messages = new object[]
                {
                new {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = $"data:{mime};base64,{b64}" } },
                        new { type = "text", text = promptWithImage }
                    }
                }
                },
                max_tokens = maxTokens,
                temperature = temperature
            };

            var raw = await InvokeAsync(endpointName, payload, ct);
            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
        }
    }
}

using AppCore.Interfaces;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Microsoft.Extensions.Logging;

namespace AppCore.Services
{
    public sealed class PdfToImagesService : IPdfToImagesService
    {
        private readonly ILogger<PdfToImagesService> _log;

        public PdfToImagesService(ILogger<PdfToImagesService> log) => _log = log;

        public Task<IReadOnlyList<string>> ConvertAsync(string pdfPath, int maxPages, int dpi, int jpgQuality, int maxWidthPx, CancellationToken ct)
        {
            var outDir = Path.Combine(Path.GetTempPath(), "img_" + Path.GetFileNameWithoutExtension(pdfPath) + "_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outDir);

            using var doc = new Document(pdfPath);
            var pages = Math.Min(doc.Pages.Count, maxPages);
            var list = new List<string>(pages);

            var res = new Resolution(dpi);
            for (int i = 1; i <= pages; i++)
            {
                ct.ThrowIfCancellationRequested();

                var page = doc.Pages[i];
                var file = Path.Combine(outDir, $"page_{i}.jpg");

                // width-bound scaling
                var b = page.Rect;
                var widthPx = Math.Min(maxWidthPx, (int)(b.Width * dpi / 72.0));
                var heightPx = (int)(b.Height * (double)widthPx / b.Width);

                var device = new JpegDevice(widthPx, heightPx, res, jpgQuality);
                using var fs = File.Create(file);
                device.Process(page, fs);
                list.Add(file);
                _log.LogInformation("Saved {File} (#{Index}/{Total})", file, i, pages);
            }
            return Task.FromResult<IReadOnlyList<string>>(list);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppCore.Interfaces
{
    public interface IPdfToImagesService
    {
        Task<IReadOnlyList<string>> ConvertAsync(string pdfPath, int maxPages, int dpi, int jpgQuality, int maxWidthPx, CancellationToken ct);
    }
}

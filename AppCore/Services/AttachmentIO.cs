using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AppCore.Services
{
    public static class AttachmentIO
    {
        private static readonly Regex _invalidChars =
            new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);

        public static string SanitizeFileName(string name) =>
            string.IsNullOrWhiteSpace(name) ? "attachment.bin" : _invalidChars.Replace(name, "_");

        public static async Task<(string savedPath, string sha256Hex, long size)> SaveAsync(
            Func<Task<Stream>> contentFactory,
            string baseFolder, DateOnly date, string graphMessageId, string preferredName,
            CancellationToken ct)
        {
            var safeName = SanitizeFileName(preferredName);
            var folder = Path.Combine(baseFolder, date.Year.ToString("0000"), date.Month.ToString("00"),
                                      date.Day.ToString("00"), graphMessageId);
            Directory.CreateDirectory(folder);
            var fullPath = Path.Combine(folder, safeName);

            string sha;
            long size;
            using (var src = await contentFactory().ConfigureAwait(false))
            using (var dest = File.Create(fullPath))
            using (var sha256 = SHA256.Create())
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await src.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
                {
                    await dest.WriteAsync(buffer.AsMemory(0, read), ct);
                    sha256.TransformBlock(buffer, 0, read, null, 0);
                }
                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                sha = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();
                size = dest.Length;
            }
            return (fullPath, sha, size);
        }
    }
}

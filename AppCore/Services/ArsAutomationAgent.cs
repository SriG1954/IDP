using AppCore.Interfaces;
using Microsoft.Playwright;

namespace AppCore.Services
{
    public class ArsAutomationAgent : IArsAutomationAgent
    {
        //private const string BaseUrl = "https://rlaars-uat.wlife.com.au:8078/ars/";
        private const string BaseUrl = "https://rlaars.wlife.com.au:8078/ars/";
        private string searchType = "FOLDER";

        private const string DownloadFolder = @"C:\Temp\ARSDownloads";

        public async Task ExecuteAsync(List<string> documentIds)
        {
            Directory.CreateDirectory(DownloadFolder);

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                     Headless = false,
                    SlowMo = 300,
                });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                AcceptDownloads = true,
                IgnoreHTTPSErrors = true
            });

            var page = await context.NewPageAsync();

            // ----------------------------------------------------
            // 1️⃣ Navigate to Base URL
            // ----------------------------------------------------
            await page.GotoAsync(BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // ----------------------------------------------------
            // 2️⃣ Login
            // ----------------------------------------------------
            await page.FillAsync("#form-username", "SRIGOV");
            await page.FillAsync("#form-password", "Holt@14000");

            await page.ClickAsync("#liSearch");

            // Instead of WaitForNavigationAsync()
            await page.WaitForURLAsync("**/arsx-main-page.html");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // ----------------------------------------------------
            // 3️⃣ Process Document IDs
            // ----------------------------------------------------
            foreach (var documentId in documentIds)
            {
                Console.WriteLine($"Processing: {documentId}");

                switch (searchType)
                {
                    case "DOCUMENT":
                        await page.FillAsync("#DocumentId", documentId);
                        await page.PressAsync("#DocumentId", "Enter");
                        break;
                    case "FOLDER":
                        await page.FillAsync("#FolderId", documentId);
                        await page.PressAsync("#FolderId", "Enter");
                        break;
                    default:
                        Console.WriteLine($"Unknown search type: {searchType}. Defaulting to DOCUMENT.");
                        await page.ClickAsync("#searchTypeDocument");
                        break;
                }

                await page.WaitForSelectorAsync("#s3SearchResult tbody tr",
                new PageWaitForSelectorOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 10000
                });

                var rows = await page.QuerySelectorAllAsync("#s3SearchResult tbody tr");

                int rowCount = rows.Count();

                if (rowCount == 0)
                {
                    Console.WriteLine("No results found.");
                    await page.FillAsync("#DocumentId", "");
                    continue;
                }
                int rowIndex = 0;
                for (int i = 0; i < rowCount; i++)
                {
                    var row = rows[i];
                    rowIndex = i + 1; // For user-friendly indexing in logs

                    // 1Get document id from column 4
                    var documentIdElement = await row.QuerySelectorAsync("td:nth-child(4)");
                    var documentPart = documentIdElement != null
                      ? SanitizeFileName((await documentIdElement.InnerTextAsync()).Trim())
                      : "UnknownDocument";

                    // 1Get file type name from column 13
                    var fileTypeElement = await row.QuerySelectorAsync("td:nth-child(13)");
                    var extensionPart = fileTypeElement != null
                      ? SanitizeFileName((await fileTypeElement.InnerTextAsync()).Trim())
                      : "UnknownDocument";

                    // 1️⃣ Get document name from column 12
                    var documentNameElement = await row.QuerySelectorAsync("td:nth-child(12)");

                    var documentName = documentNameElement != null
                        ? SanitizeFileName((await documentNameElement.InnerTextAsync()).Trim())
                        : "UnknownDocument";

                    // 2️⃣ Get download link from column 1
                    var downloadLink = await row.QuerySelectorAsync("td:nth-child(1) a");

                    if (downloadLink == null)
                        continue;

                    // 3️⃣ Trigger download
                    var download = await page.RunAndWaitForDownloadAsync(async () =>
                    {
                        await downloadLink.ClickAsync();
                    });

                    string suggestedFileName = string.Empty;
                    switch (searchType)
                    {
                        case "DOCUMENT":
                            suggestedFileName = $"ARS_{documentPart}_{rowIndex}.{extensionPart}";

                            break;
                        case "FOLDER":
                            suggestedFileName = $"ARS_{documentId}_{documentPart}_{rowIndex}.{extensionPart}";

                            break;
                        default:
                            Console.WriteLine($"Unknown search type: {searchType}. Defaulting to DOCUMENT.");
                            await page.ClickAsync("#searchTypeDocument");
                            break;
                    }

                    // 4️⃣ Save using document name
                    //var extension = Path.GetExtension(download.SuggestedFilename);
                    //var finalPath = Path.Combine(DownloadFolder, documentName + extension);

                    var finalPath = Path.Combine(DownloadFolder, suggestedFileName);

                    await download.SaveAsAsync(finalPath);

                    //Console.WriteLine($"Saved as: {documentName + extension}");
                    Console.WriteLine($"Saved as: {suggestedFileName}");

                }

                // 🔄 Refresh page for next documentId
                await page.ReloadAsync();

                // Wait until input becomes enabled again
                await page.WaitForSelectorAsync("#DocumentId:not([disabled])");

                // Clear input for next iteration
                await page.FillAsync("#DocumentId", "");
            }

            // ----------------------------------------------------
            // 4️⃣ Logout
            // ----------------------------------------------------
            await page.ClickAsync("#menu-logout ul li a");

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await browser.CloseAsync();
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName;
        }
    }
}

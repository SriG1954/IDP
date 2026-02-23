using AppCore.Interfaces;
using Microsoft.Playwright;

namespace AppCore.Services
{
    public class ArsAutomationAgent : IArsAutomationAgent
    {
        private const string BaseUrl = "https://rlaars-uat.wlife.com.au:8078/ars/";
        private const string DownloadFolder = @"C:\Temp\ARSDownloads";

        public async Task ExecuteAsync(List<string> documentIds)
        {
            Directory.CreateDirectory(DownloadFolder);

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
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
            await page.FillAsync("#form-username", "your-userid");
            await page.FillAsync("#form-password", "your-password");

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

                await page.FillAsync("#DocumentId", documentId);
                await page.PressAsync("#DocumentId", "Enter");

                await page.WaitForSelectorAsync("#s3SearchResult");
                await page.WaitForSelectorAsync("#s3SearchResult tbody tr");

                var rows = await page.QuerySelectorAllAsync("#s3SearchResult tbody tr");

                if (rows.Count == 0)
                {
                    Console.WriteLine("No results found.");
                    await page.FillAsync("#DocumentId", "");
                    continue;
                }

                foreach (var row in rows)
                {
                    var link = await row.QuerySelectorAsync("td:nth-child(1) a");
                    if (link == null)
                        continue;

                    var download = await page.RunAndWaitForDownloadAsync(async () =>
                    {
                        await link.ClickAsync();
                    });

                    var savePath = Path.Combine(DownloadFolder, download.SuggestedFilename);

                    await download.SaveAsAsync(savePath);

                    Console.WriteLine($"Downloaded: {download.SuggestedFilename}");
                }

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
    }
}

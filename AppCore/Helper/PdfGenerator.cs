using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.IO;
using System.Reflection;


namespace AppCore.Helper
{
    public static class PdfGenerator
    {
        public static async Task<bool> GeneraeTestPdfDocumentsV1(int totalNumber, string folderPath)
        {
            bool success = false;
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            try
            {
                if (totalNumber <= 0)
                {
                    totalNumber = 100; // default to 10 documents
                }

                if (folderPath == null)
                {
                    throw new ArgumentNullException(nameof(folderPath), "Folder path cannot be null.");
                }

                // Ensure folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                await Task.Delay(100); // Simulate async work if needed

                for (int i = 1; i <= totalNumber; i++)
                {
                    string fileName = Path.Combine(folderPath, $"Document{i}.pdf");

                    using (PdfDocument document = new PdfDocument())
                    {
                        document.Info.Title = $"Document {i}";
                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        try
                        {
                            // Title
                            XFont titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
                            gfx.DrawString($"Sample PDF Document {i}", titleFont, XBrushes.DarkBlue,
                                new XRect(0, 20, page.Width.Point, 40), XStringFormats.TopCenter);

                            List<string> lines = new List<string>();
                            lines.Add("");
                            lines.Add("This is a sample PDF document generated programmatically.");
                            lines.Add("");
                            lines.Add("This document contains some text for testing bulk PDF creation.");
                            lines.Add($"Document number: {i} is generated for HARD DELETE use case testing.");
                            lines.Add("");
                            lines.Add("");
                            lines.Add("HARD DELETE service is designed to read both the Source (A) and");
                            lines.Add("Delete metadata (B) rows for a batchname, finds the difference (A-B)");
                            lines.Add("Create a new object from the difference meta data range, and");
                            lines.Add("Finally saves the object with the same key which is the batchname.");
                            lines.Add("");
                            lines.Add("");
                            lines.Add($"Generated on: {DateTime.Now.ToString()}");
                            lines.Add("");
                            lines.Add("");

                            // Body text
                            XFont bodyFont = new XFont("Arial", 12, XFontStyleEx.Regular);
                            double x = 40;   // left margin
                            double y = 100;  // starting top position
                            double lineHeight = bodyFont.GetHeight(); // line spacing

                            foreach (string line in lines)
                            {
                                gfx.DrawString(line, bodyFont, XBrushes.Black,
                                    new XRect(x, y, page.Width.Point - 80, page.Height.Point - 100),
                                    XStringFormats.TopLeft);
                                y += lineHeight;
                            }

                            // Pick 10–50 random chunks for this doc
                            List<string> textChunks = GeneratePhilosophyChunks();

                            Random rnd = new Random();
                            int chunkCount = rnd.Next(10, 50);
                            var chosenChunks = textChunks.OrderBy(x => rnd.Next()).Take(chunkCount).ToList();

                            x = 40;  // left margin
                            y = y + 50;  // starting top position
                            lineHeight = bodyFont.GetHeight();

                            foreach (string chunk in chosenChunks)
                            {
                                gfx.DrawString(chunk, bodyFont, XBrushes.Black,
                                       new XRect(x, y, page.Width.Point - 80, page.Height.Point - 100),
                                       XStringFormats.TopLeft);
                                y += lineHeight;

                                // If near page bottom → new page
                                if (y > page.Height.Point - 60)
                                {
                                    page = document.AddPage();
                                    gfx.Dispose();
                                    gfx = XGraphics.FromPdfPage(page);
                                    y = 60;
                                }

                                // Replace this line:
                                // if (y > page.Height - 60)

                                // With this:
                                if (y > page.Height.Point - 60)
                                    y += lineHeight; // extra spacing between chunks
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating content for Document {i}: " + ex.Message);
                        }
                        finally
                        {
                            gfx.Dispose();
                        }

                        document.Save(fileName);
                    }

                    Console.WriteLine($"Created: {fileName}");
                }
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating PDF documents: " + ex.Message);
                success = false;
            }

            return success;
        }

        public static async Task<bool> GeneraeTestPdfDocuments(int totalNumber, string folderPath)
        {
            bool success = false;
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            try
            {
                if (totalNumber <= 0)
                {
                    totalNumber = 100; // default to 10 documents
                }

                if (folderPath == null)
                {
                    throw new ArgumentNullException(nameof(folderPath), "Folder path cannot be null.");
                }

                // Ensure folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                await Task.Delay(100); // Simulate async work if needed

                for (int i = 1; i <= totalNumber; i++)
                {
                    string fileName = Path.Combine(folderPath, $"Document{i}.pdf");

                    using (PdfDocument document = new PdfDocument())
                    {
                        document.Info.Title = $"Document {i}";
                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        try
                        {
                            // Title
                            XFont titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
                            gfx.DrawString($"Sample PDF Document {i}", titleFont, XBrushes.DarkBlue,
                                new XRect(0, 20, page.Width.Point, 40), XStringFormats.TopCenter);

                            List<string> lines = new List<string>();
                            lines.Add("");
                            lines.Add("This is a sample PDF document generated programmatically.");
                            lines.Add("");
                            lines.Add("This document contains some text for testing bulk PDF creation.");
                            lines.Add($"Document number: {i} is generated for HARD DELETE use case testing.");
                            lines.Add("");
                            lines.Add("");
                            lines.Add("HARD DELETE service is designed to read both the Source (A) and");
                            lines.Add("Delete metadata (B) rows for a batchname, finds the difference (A-B)");
                            lines.Add("Create a new object from the difference meta data range, and");
                            lines.Add("Finally saves the object with the same key which is the batchname.");
                            lines.Add("");
                            lines.Add("");
                            lines.Add($"Generated on: {DateTime.Now.ToString()}");
                            lines.Add("");
                            lines.Add("");

                            // Body text
                            XFont bodyFont = new XFont("Arial", 12, XFontStyleEx.Regular);
                            double x = 40;   // left margin
                            double y = 100;  // starting top position
                            double lineHeight = bodyFont.GetHeight(); // line spacing

                            foreach (string line in lines)
                            {
                                gfx.DrawString(line, bodyFont, XBrushes.Black,
                                    new XRect(x, y, page.Width.Point - 80, page.Height.Point - 100),
                                    XStringFormats.TopLeft);
                                y += lineHeight;
                            }

                            // Pick 10–50 random chunks for this doc
                            List<string> textChunks = GeneratePhilosophyChunks();

                            Random rnd = new Random();
                            int chunkCount = rnd.Next(10, 50);
                            var chosenChunks = textChunks.OrderBy(x => rnd.Next()).Take(chunkCount).ToList();

                            x = 40;  // left margin
                            y = y + 50;  // starting top position
                            lineHeight = bodyFont.GetHeight();

                            foreach (string chunk in chosenChunks)
                            {
                                gfx.DrawString(chunk, bodyFont, XBrushes.Black,
                                       new XRect(x, y, page.Width.Point - 80, page.Height.Point - 100),
                                       XStringFormats.TopLeft);
                                y += lineHeight;

                                // If near page bottom → new page
                                if (y > page.Height.Point - 60)
                                {
                                    page = document.AddPage();
                                    gfx.Dispose();
                                    gfx = XGraphics.FromPdfPage(page);
                                    y = 60;
                                }

                                // Replace this line:
                                // if (y > page.Height - 60)

                                // With this:
                                if (y > page.Height.Point - 60)
                                    y += lineHeight; // extra spacing between chunks
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating content for Document {i}: " + ex.Message);
                        }
                        finally
                        {
                            gfx.Dispose();
                        }

                        document.Save(fileName);
                    }

                    Console.WriteLine($"Created: {fileName}");
                }
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating PDF documents: " + ex.Message);
                success = false;
            }

            return success;
        }

        public static async Task<bool> GeneraeteTestTextDocuments(int totalNumber, string folderPath)
        {
            bool success = true;
            try
            {
                // Ensure folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                await Task.Delay(100); // Simulate async work if needed

                for (int i = 1; i <= totalNumber; i++)
                {
                    string fileName = Path.Combine(folderPath, $"Document{i}.txt");
                    // Create and write to the file
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        writer.WriteLine($"Welcome to text file generation {i.ToString()}");
                        writer.WriteLine("");

                        List<string> lines = new List<string>();
                        lines.Add("");
                        lines.Add("This is a sample PDF document generated programmatically.");
                        lines.Add("");
                        lines.Add("This document contains some text for testing bulk PDF creation.");
                        lines.Add($"Document number: {i} is generated for HARD DELETE use case testing.");
                        lines.Add("");
                        lines.Add("");
                        lines.Add("HARD DELETE service is designed to read both the Source (A) and");
                        lines.Add("Delete metadata (B) rows for a batchname, finds the difference (A-B)");
                        lines.Add("Create a new object from the difference meta data range, and");
                        lines.Add("Finally saves the object with the same key which is the batchname.");
                        lines.Add("");
                        lines.Add("");
                        lines.Add($"Generated on: {DateTime.Now.ToString()}");
                        lines.Add("");
                        lines.Add("");

                        foreach (string line in lines)
                        {
                            writer.WriteLine(line);
                        }

                        writer.WriteLine("");
                        List<string> textChunks = GeneratePhilosophyChunks();
                        foreach (string line in textChunks)
                        {
                            writer.WriteLine(line);
                        }
                    }

                    Console.WriteLine($"Text file {i.ToString()} is created successfully.");
                }

            }
            catch (Exception ex)
            {
                success = false;
                Console.WriteLine(ex.Message);
            }
            return success;
        }

        static List<string> GeneratePhilosophyChunks()
        {
            return new List<string>
            {
                "Plato: The unexamined life is not worth living.",
                "Aristotle: It is the mark of an educated mind to be able to entertain a thought without accepting it.",
                "Epictetus: Wealth consists not in having great possessions, but in having few wants.",
                "Marcus Aurelius: The happiness of your life depends upon the quality of your thoughts.",
                "Descartes: I think, therefore I am.",
                "Kant: Science is organized knowledge. Wisdom is organized life.",
                "Hume: Reason is, and ought only to be the slave of the passions.",
                "Nietzsche: He who has a why to live can bear almost any how.",
                "Nietzsche: That which does not kill us makes us stronger.",
                "Socrates: To find yourself, think for yourself.",
                "John Stuart Mill: Genuine justice requires impartiality.",
                "Locke: No man’s knowledge here can go beyond his experience.",
                "Berkeley: To be is to be perceived.",
                "Spinoza: The highest activity a human being can attain is learning for understanding, because to understand is to be free.",
                "Schopenhauer: Compassion is the basis of morality.",
                "Russell: The good life is one inspired by love and guided by knowledge.",
                "Wittgenstein: The limits of my language mean the limits of my world.",
                "Kierkegaard: Life can only be understood backwards; but it must be lived forwards.",
                "Heidegger: Being is the most universal and the emptiest concept.",
                "Sartre: Man is condemned to be free.",
                "Camus: The only way to deal with an unfree world is to become so absolutely free that your very existence is an act of rebellion.",
                "Camus: In the depth of winter, I finally learned that within me there lay an invincible summer.",
                "Aristotle: Happiness depends upon ourselves.",
                "Plato: Opinion is the medium between knowledge and ignorance.",
                "Hobbes: The condition of man is a condition of war of everyone against everyone.",
                "Rousseau: Man is born free, and everywhere he is in chains.",
                "Machiavelli: It is better to be feared than loved, if you cannot be both.",
                "Seneca: It is not that we have a short time to live, but that we waste much of it.",
                "Augustine: Faith is to believe what you do not see; the reward of this faith is to see what you believe.",
                "Pascal: The heart has its reasons of which reason knows nothing.",
                "Kant: Act only according to that maxim whereby you can at the same time will that it should become a universal law.",
                "Hume: Custom is the great guide of human life.",
                "Spinoza: All things excellent are as difficult as they are rare.",
                "Russell: Fear is the main source of superstition, and one of the main sources of cruelty.",
                "Dewey: Education is not preparation for life; education is life itself.",
                "Whitehead: The art of progress is to preserve order amid change and to preserve change amid order.",
                "Arendt: The most radical revolutionary will become a conservative the day after the revolution.",
                "Rawls: Justice is the first virtue of social institutions.",
                "Nozick: Individuals have rights, and there are things no person or group may do to them.",
                "Popper: Unlimited tolerance must lead to the disappearance of tolerance.",
                "Marcuse: Free election of masters does not abolish the masters or the slaves.",
                "Habermas: Knowledge and human interests are interwoven.",
                "Simone de Beauvoir: One is not born, but rather becomes, a woman.",
                "Foucault: Where there is power, there is resistance.",
                "Derrida: There is nothing outside the text.",
                "Levinas: The Other is the very locus of ethics.",
                "Nietzsche: To live is to suffer, to survive is to find some meaning in the suffering.",
                "Aristotle: The whole is greater than the sum of its parts.",
            };

            //static IEnumerable<string> WrapText(string text, int maxChars)
            //{
            //    if (string.IsNullOrWhiteSpace(text))
            //        yield break;

            //    var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    var line = new System.Text.StringBuilder();

            //    foreach (var word in words)
            //    {
            //        if (line.Length + word.Length + 1 > maxChars)
            //        {
            //            yield return line.ToString().Trim();
            //            line.Clear();
            //        }
            //        line.Append(word).Append(' ');
            //    }

            //    if (line.Length > 0)
            //        yield return line.ToString().Trim();
            //}
        }

        static void DrawFirstPage(XGraphics gfx, int i)
        {
            // Title
            XFont titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
            gfx.DrawString($"Sample PDF Document {i}", titleFont, XBrushes.DarkBlue,
                new XRect(0, 40, 500, 40), XStringFormats.TopCenter);

            List<string> lines = new List<string>();
            lines.Add("");
            lines.Add("This is a sample PDF document generated programmatically.");
            lines.Add("");
            lines.Add("This document contains some text for testing bulk PDF creation.");
            lines.Add($"Document number: {i} is generated for HARD DELETE use case testing.");
            lines.Add("");
            lines.Add("");
            lines.Add("HARD DELETE service is designed to read both the Source (A) and");
            lines.Add("Delete metadata (B) rows for a batchname, finds the difference (A-B)");
            lines.Add("Create a new object from the difference meta data range, and");
            lines.Add("Finally saves the object with the same key which is the batchname.");
            lines.Add("");
            lines.Add("");
            lines.Add($"Generated on: {DateTime.Now.ToString()}");
            lines.Add("");
            lines.Add("");

            // Body text
            XFont bodyFont = new XFont("Arial", 12, XFontStyleEx.Regular);
            //double x = 40;   // left margin
            double y = 100;  // starting top position
            double lineHeight = bodyFont.GetHeight(); // line spacing

            foreach (string line in lines)
            {
                gfx.DrawString(line, bodyFont, XBrushes.Black,
                    new XRect(100, 100, 500, 700),
                    XStringFormats.TopLeft);
                y += lineHeight;
            }

        }

        static void DrawTemplatePage(XGraphics gfx)
        {
            var font = new XFont("Arial", 12, XFontStyleEx.Regular);

            string longText = string.Join(" ",
                Enumerable.Repeat(
                    "This is a sample PDF page used for file size testing. " +
                    "The same page content is duplicated multiple times. " +
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789",
                    30));

            var rect = new XRect(40, 40, 500, 750);
            gfx.DrawString(longText, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        static byte[] CreatePdfBetween2And2_5MB()
        {
            using var document = new PdfDocument();

            byte[] pdfBytes;

            do
            {
                var page = document.AddPage();
                using var gfx = XGraphics.FromPdfPage(page);

                DrawTemplatePage(gfx);

                using var ms = new MemoryStream();
                document.Save(ms, false);
                pdfBytes = ms.ToArray();

            } while (pdfBytes.Length < 2 * 1024 * 1024); // 2 MB

            return pdfBytes;
        }

    }
}

public class CustomFontResolver : IFontResolver
{
    public string DefaultFontName => "Arial";

    public byte[] GetFont(string faceName)
    {
        // Map all to Arial.ttf in Windows Fonts folder
        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
        return File.ReadAllBytes(fontPath);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Map Helvetica -> Arial
        if (familyName.Equals("Helvetica", StringComparison.OrdinalIgnoreCase))
        {
            return new FontResolverInfo("Arial");
        }

        return new FontResolverInfo("Arial");
    }
}





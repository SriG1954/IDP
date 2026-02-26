using AppCore.Data;
using AppCore.EntityModels;
using AppCore.Interfaces;
using AppCore.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AppCore.Services
{
    public class ClassifierService : IClassifierService
    {
        private readonly IS3Service _s3;
        private readonly IPdfToImagesService _pdf;
        private readonly ITextractService _tex;
        private readonly IVLMService _vlm;
        private readonly IPromptRepository _cfg;
        private readonly AppDbContext _db;
        private readonly ILogger<ClassifierService> _log;

        private const int MAX_IMAGE_KB_FOR_PROCESSING = 450;

        public ClassifierService(IS3Service s3, IPdfToImagesService pdf, ITextractService tex,
                                 IVLMService vlm, IPromptRepository cfg, AppDbContext db,
                                 ILogger<ClassifierService> log)
        { _s3 = s3; _pdf = pdf; _tex = tex; _vlm = vlm; _cfg = cfg; _db = db; _log = log; }

        public async Task RunJobAsync(DocumentJob job, CancellationToken ct)
        {
            job.Status = "Running"; job.StartedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            var timings = new Dictionary<string, object>();
            try
            {
                var pdfPath = await _s3.DownloadPdfAsync(job.S3Bucket, job.S3Key, ct);

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var images = await _pdf.ConvertAsync(pdfPath, job.MaxPages, dpi: 150, jpgQuality: 80, maxWidthPx: 1500, ct);
                timings["pdf_to_images_ms"] = sw.ElapsedMilliseconds;

                var kvPrompt = await _cfg.GetActivePromptAsync("KV_BASE_INSTRUCTIONS", ct);
                var (endpoint, _, maxTokens, temp) = await _cfg.GetActiveEndpointAsync("Qwen2-VL", ct);
                var pages = new List<Dictionary<string, object>>();
                var kvSummaries = new List<Dictionary<string, object>>();
                long totalTexMs = 0, totalLlmMs = 0;

                // limit concurrency to 3
                using var sem = new SemaphoreSlim(3);
                var tasks = images.Select(async (img, idx) =>
                {
                    await sem.WaitAsync(ct);
                    try
                    {
                        var texSw = System.Diagnostics.Stopwatch.StartNew();
                        var tex = await _tex.DetectLinesAsync(img, ct);
                        texSw.Stop();
                        Interlocked.Add(ref totalTexMs, (long)texSw.Elapsed.TotalMilliseconds);

                        Dictionary<string, object>? kvJson = null;
                        string? kvRaw = null;
                        var fileKb = new FileInfo(img).Length / 1024.0;
                        if (fileKb <= MAX_IMAGE_KB_FOR_PROCESSING && idx < job.KvPagesLimit)
                        {
                            var llmSw = System.Diagnostics.Stopwatch.StartNew();
                            kvRaw = await _vlm.CallKvAsync(endpoint, kvPrompt, tex, img, maxTokens: 1500, temperature: temp, ct);
                            llmSw.Stop();
                            Interlocked.Add(ref totalLlmMs, (long)llmSw.Elapsed.TotalMilliseconds);

                            // best-effort parse
                            kvJson = TryParseJson(kvRaw) ?? new Dictionary<string, object> { ["raw_response"] = kvRaw };
                        }

                        lock (pages)
                        {
                            pages.Add(new Dictionary<string, object>
                            {
                                ["page_index"] = idx + 1,
                                ["image_path"] = img,
                                ["image_size_kb"] = fileKb,
                                ["textract"] = tex,
                                ["kv_result"] = kvJson!
                            });

                            if (kvJson != null)
                            {
                                kvSummaries.Add(new Dictionary<string, object>
                                {
                                    ["attachment_id"] = job.JobId.ToString(),
                                    ["filename"] = Path.GetFileName(pdfPath),
                                    ["page_index"] = idx + 1,
                                    ["kv_json_text"] = JsonSerializer.Serialize(kvJson)
                                });
                            }
                        }
                    }
                    finally { sem.Release(); }
                }).ToArray();

                await Task.WhenAll(tasks);

                timings["textract_total_ms"] = totalTexMs;
                timings["kv_llm_total_ms"] = totalLlmMs;

                // Build classifier input (pages_text + kv_summaries + structured_flags)
                var classifierInput = BuildClassifierInput(job.ChannelType, pages, kvSummaries);
                var classifierPrompt = await _cfg.GetActivePromptAsync("CLASSIFIER_INSTRUCTIONS", ct);
                var catalogJson = await _cfg.GetActiveCatalogJsonAsync(ct);

                var firstImage = images.First();
                var finalPrompt = classifierPrompt + "\n\nCATALOG:\n" + catalogJson + "\n\nINPUT:\n" +
                                  JsonSerializer.Serialize(classifierInput, new JsonSerializerOptions { WriteIndented = true });

                var (endpoint2, _, maxTokens2, temp2) = await _cfg.GetActiveEndpointAsync("Qwen2-VL", ct);

                // correct the missing params
                var rawClassifier = await _vlm.CallClassifierAsync(endpoint2, finalPrompt, "", "", firstImage, 4000, temp2, ct);

                var classification = TryParseJson(rawClassifier) ?? new Dictionary<string, object>
                {
                    ["error"] = "Classifier did not return valid JSON",
                    ["raw_response"] = rawClassifier
                };

                // catalog scoring + overrides (port of your Python)
                var (scores, updated) = ApplyCatalogScoringAndOverrides(classifierInput, catalogJson, classification);
                // store
                var res = new DocumentResult
                {
                    JobId = job.JobId,
                    ClassificationJson = JsonSerializer.Serialize(updated),
                    StructuredFlagsJson = JsonSerializer.Serialize(classifierInput["structured_flags"]),
                    CatalogScoresJson = JsonSerializer.Serialize(scores),
                    TimingsJson = JsonSerializer.Serialize(timings)
                };
                _db.DocumentResults.Add(res);
                job.Status = "Succeeded"; job.CompletedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                job.Status = "Failed"; job.CompletedAtUtc = DateTime.UtcNow; job.Error = ex.ToString();
                await _db.SaveChangesAsync(ct);
                _log.LogError(ex, "Job {JobId} failed", job.JobId);
            }
        }

        private static Dictionary<string, object>? TryParseJson(string raw)
        {
            try { return JsonSerializer.Deserialize<Dictionary<string, object>>(raw); }
            catch { return null; }
        }

        // --- build input + flags (shortened faithful port of your Python) ---
        private static Dictionary<string, object> BuildClassifierInput(string channelType,
            List<Dictionary<string, object>> pages,
            List<Dictionary<string, object>> kvSummaries)
        {
            // pages_text
            var pagesText = new List<Dictionary<string, object>>();
            foreach (var p in pages)
            {
                var textract = (JsonElement)p["textract"];
                var lines = new List<string>();
                if (textract.ValueKind == JsonValueKind.Object && textract.TryGetProperty("Blocks", out var blocks))
                {
                    foreach (var b in blocks.EnumerateArray())
                    {
                        if (b.TryGetProperty("Text", out var t) && t.ValueKind == JsonValueKind.String)
                            lines.Add(t.GetString()!);
                    }
                }
                pagesText.Add(new()
                {
                    ["attachment_id"] = "att",
                    ["filename"] = p["image_path"],
                    ["page_index"] = p["page_index"],
                    ["text"] = string.Join("\n", lines),
                    ["role"] = "primary_form" // close to your classify_doc_role
                });
            }

            // structured flags derived from kv
            var flags = DeriveStructuredFlagsFromKv(kvSummaries);

            return new()
            {
                ["channel_type"] = channelType,
                ["pages_text"] = pagesText,
                ["kv_summaries"] = kvSummaries.Take(30).ToList(),
                ["structured_flags"] = flags
            };
        }

        private static List<Dictionary<string, object>> DeriveStructuredFlagsFromKv(List<Dictionary<string, object>> kvSummaries)
        {
            var flags = new List<Dictionary<string, object>>();
            foreach (var kv in kvSummaries)
            {
                try
                {
                    var text = kv["kv_json_text"]?.ToString() ?? "{}";
                    using var doc = JsonDocument.Parse(text);
                    // HIGH-TRUST PaymentOptions
                    if (TryFindPaymentOptions(doc.RootElement, out bool? full, out bool? partial))
                    {
                        if (full == true && partial == false)
                            flags.Add(MkFlag(kv, "withdrawal_mode", "FULL", "PaymentOptions.FullWithdrawal=true, PartialWithdrawal=false"));
                        else if (full == false && partial == true)
                            flags.Add(MkFlag(kv, "withdrawal_mode", "PARTIAL", "PaymentOptions.FullWithdrawal=false, PartialWithdrawal=true"));
                    }
                    // FormName flag
                    if (TryFindFormName(doc.RootElement, out var form))
                    {
                        flags.Add(MkFlag(kv, "form_name", form, $"FormName={form}"));
                    }
                }
                catch { /* ignore bad kv */ }
            }
            return flags;

            static Dictionary<string, object> MkFlag(Dictionary<string, object> kv, string name, string value, string evidence)
                => new()
                {
                    ["attachment_id"] = kv["attachment_id"]!,
                    ["filename"] = kv["filename"]!,
                    ["page_index"] = kv["page_index"]!,
                    ["name"] = name,
                    ["flag_type"] = name,
                    ["value"] = value,
                    ["evidence"] = evidence
                };

            static bool TryFindPaymentOptions(JsonElement root, out bool? full, out bool? partial)
            {
                full = partial = null;
                foreach (var p in root.EnumerateObject())
                {
                    if (p.NameEquals("PaymentOptions") && p.Value.ValueKind == JsonValueKind.Object)
                    {
                        if (p.Value.TryGetProperty("FullWithdrawal", out var f))
                            full = f.ValueKind == JsonValueKind.True ? true :
                                   f.ValueKind == JsonValueKind.False ? false :
                                   f.ValueKind == JsonValueKind.String && f.GetString()!.ToLower().Contains("full") &&
                                   !f.GetString()!.ToLower().Contains("partial");
                        if (p.Value.TryGetProperty("PartialWithdrawal", out var q))
                            partial = q.ValueKind == JsonValueKind.True ? true :
                                      q.ValueKind == JsonValueKind.False ? false :
                                      q.ValueKind == JsonValueKind.String && q.GetString()!.ToLower().Contains("partial");
                        return true;
                    }
                    if (p.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        if (TryFindPaymentOptions(p.Value, out full, out partial)) return true;
                    }
                }
                return false;
            }

            static bool TryFindFormName(JsonElement root, out string value)
            {
                foreach (var p in root.EnumerateObject())
                {
                    if (p.NameEquals("kv_pairs") && p.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in p.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.Object &&
                                item.TryGetProperty("key", out var k) &&
                                item.TryGetProperty("value", out var v) &&
                                k.ValueKind == JsonValueKind.String &&
                                v.ValueKind == JsonValueKind.String &&
                                k.GetString()!.Replace(" ", "", StringComparison.OrdinalIgnoreCase)
                                  .Equals("formname", StringComparison.OrdinalIgnoreCase))
                            {
                                value = v.GetString()!;
                                return true;
                            }
                        }
                    }
                    if (p.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        if (TryFindFormName(p.Value, out value)) return true;
                    }
                }
                value = "";
                return false;
            }
        }

        private static (List<Dictionary<string, object>> scores, Dictionary<string, object> updated)
            ApplyCatalogScoringAndOverrides(Dictionary<string, object> input, string catalogJson, Dictionary<string, object> classification)
        {
            var scores = ComputeCatalogScoresFromText((JsonElement)input["pages_text"], catalogJson);
            var updated = classification;

            // Example small override: if LLM gave No Match or blank, pick top score ≥ 1.0
            if (scores.Count > 0)
            {
                var top = scores[0];
                if (!updated.TryGetValue("primary_classification", out var pcObj) || pcObj is not JsonElement)
                    updated["primary_classification"] = new Dictionary<string, object>();

                var pc = pcObj as Dictionary<string, object> ?? new();
                string wt = pc.TryGetValue("worktype", out var w) ? w?.ToString() ?? "" : "";
                string swt = pc.TryGetValue("subworktype", out var s) ? s?.ToString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(wt) || string.IsNullOrWhiteSpace(swt) || wt == "No Match" || swt == "No Match")
                {
                    if (top.TryGetValue("score", out var sc) && Convert.ToDouble(sc) >= 1.0)
                    {
                        pc["worktype"] = top["worktype"]!;
                        pc["subworktype"] = top["subworktype"]!;
                        pc["rationale"] = "Overridden by catalog scoring due to low/blank LLM match.";
                        updated["primary_classification"] = pc;
                    }
                }
            }
            return (scores, updated);
        }

        private static List<Dictionary<string, object>> ComputeCatalogScoresFromText(JsonElement pagesText, string catalogJson)
        {
            string combined = string.Join("\n", pagesText.EnumerateArray()
                                    .Select(e => e.GetProperty("text").GetString() ?? ""));
            var ws = Regex.Replace(combined.ToLowerInvariant(), "\\s+", " ");

            var catalog = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(catalogJson) ?? new();
            var results = new List<Dictionary<string, object>>();

            foreach (var row in catalog)
            {
                var wt = row.GetValueOrDefault("worktype")?.ToString() ?? "";
                var swt = row.GetValueOrDefault("subworktype")?.ToString() ?? "";
                var pos = row.GetValueOrDefault("positive_indicators")?.ToString();
                var neg = row.GetValueOrDefault("negative_indicators")?.ToString();
                var priority = Convert.ToInt32(row.GetValueOrDefault("priority") ?? 50);

                int hits = 0, negHits = 0;
                if (!string.IsNullOrEmpty(pos))
                {
                    var items = JsonSerializer.Deserialize<List<string>>(pos) ?? new();
                    foreach (var p in items) hits += Count(ws, p.ToLowerInvariant());
                }
                if (!string.IsNullOrEmpty(neg))
                {
                    var items = JsonSerializer.Deserialize<List<string>>(neg) ?? new();
                    foreach (var p in items) negHits += Count(ws, p.ToLowerInvariant());
                }

                if (hits == 0 && negHits == 0) continue;

                var score = Math.Max(hits - negHits * 5, 0) * (priority / 50.0);
                results.Add(new() { ["worktype"] = wt, ["subworktype"] = swt, ["score"] = score, ["hits"] = hits, ["neg_hits"] = negHits });
            }
            return results.OrderByDescending(r => Convert.ToDouble(r["score"])).ToList();

            static int Count(string text, string phrase) => string.IsNullOrWhiteSpace(phrase) ? 0 : text.Split(phrase).Length - 1;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace KannadaNudiEditor.Helpers
{
    public static class FileConversionService
    {
        public static string AsciiToUnicodeConverter(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    SimpleLogger.Log("[A2U] Empty input.");
                    return input ?? string.Empty;
                }

                var cfg = Config.Value;
                //SimpleLogger.Log($"[A2U] Start | chars={input.Length} | maxToken={cfg.MaxTokenLength}");

                string pre = PreInsertZwnj(input, cfg);
                string mapped = ProcessParagraphPreserveSpacing(pre, cfg);

                // 1) Fix conjunct/vowel-sign ordering (JS vattakshara reorder equivalent). [file:41]
                string normalized = PostProcessKannadaClusters(mapped, cfg);

                // 2) Final small fixups (JS deergha/postprocess equivalent). [file:41]
                string finalText = ApplyPostFixups(normalized, cfg);

                SimpleLogger.Log($"[A2U] Done  | chars={finalText.Length}");
                return finalText;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[A2U] Failed");
                return input ?? string.Empty;
            }
        }

        public static string UnicodeToAsciiConverter(string input)
        {
            SimpleLogger.Log("[U2A] Placeholder - not implemented.");
            return input ?? string.Empty;
        }

        // ============================================================
        // Config load (cached)
        // ============================================================
        private static readonly Lazy<AsciiToUnicodeConfig> Config = new(LoadConfig);

        private static AsciiToUnicodeConfig LoadConfig()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string jsonPath = Path.Combine(baseDir, "Resources", "AsciiToUnicodeMapping.json");

            SimpleLogger.Log($"[A2U] Loading mapping JSON: {jsonPath}");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("AsciiToUnicodeMapping.json not found.", jsonPath);

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            var root = JsonSerializer.Deserialize<AsciiToUnicodeJson>(json, opts)
                       ?? throw new InvalidOperationException("Failed to deserialize AsciiToUnicodeMapping.json.");

            var cfg = AsciiToUnicodeConfig.From(root);

            SimpleLogger.Log($"[A2U] Mapping loaded | mapping={cfg.Mapping.Count} | vattu={cfg.Vattaksharagalu.Count} | broken={cfg.BrokenCases.Count} | fixups={cfg.PostFixups.Count}");
            return cfg;
        }

        // ============================================================
        // Preprocess: ZWNJ insertion (JS REGEXASCIIZWNJ intent) [file:41]
        // ============================================================
        private static string PreInsertZwnj(string input, AsciiToUnicodeConfig cfg)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.IndexOf(cfg.AsciiHalantChar) < 0) return input;

            var sb = new StringBuilder(input.Length + 16);
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                sb.Append(ch);

                if (ch == cfg.AsciiHalantChar && i + 1 < input.Length)
                {
                    char next = input[i + 1];
                    if (cfg.AsciiConsonantStartChars.Contains(next))
                        sb.Append(cfg.Zwnj);
                }
            }
            return sb.ToString();
        }

        // ============================================================
        // Core conversion: streaming longest token match
        // ============================================================
        private static string ProcessParagraphPreserveSpacing(string text, AsciiToUnicodeConfig cfg)
        {
            if (text == null) return string.Empty;

            int i = 0;
            int maxLen = text.Length;
            var op = new List<string>(capacity: Math.Min(128, maxLen * 2));

            while (i < maxLen)
            {
                if (cfg.IgnoreList.Contains(text[i]))
                {
                    i++;
                    continue;
                }

                var (jump, newOp) = FindMapping(op, text, i, cfg);
                op = newOp;
                i += 1 + jump;
            }

            return string.Concat(op);
        }

        private static (int jump, List<string> op) FindMapping(List<string> op, string txt, int currentPos, AsciiToUnicodeConfig cfg)
        {
            int remaining = txt.Length - currentPos;

            int maxLen = cfg.MaxTokenLength;
            if (remaining < (maxLen + 1))
                maxLen = remaining - 1;
            if (maxLen < 0)
                maxLen = 0;

            int n = 0;

            for (int i = maxLen; i >= 0; i--)
            {
                string t = txt.Substring(currentPos, i + 1);

                if (cfg.Mapping.TryGetValue(t, out string mapped))
                {
                    if (op.Count > 0 && EndsWithHalant(op[^1], cfg.Halant))
                        op.Add(cfg.Zwj.ToString());

                    op.Add(mapped);
                    n = i;
                    break;
                }

                if (i > 0) continue;

                if (cfg.AsciiArkavattu.ContainsKey(t))
                    op = ProcessArkavattu(op, t, cfg);
                else if (cfg.Vattaksharagalu.ContainsKey(t))
                    op = ProcessVattakshara(op, t, cfg);
                else if (cfg.BrokenCases.ContainsKey(t))
                    op = ProcessBrokenCase(op, t, cfg);
                else
                {
                    if (t.Length == 1 && t[0] == cfg.AsciiHalantChar)
                        op.Add(cfg.Halant.ToString());
                    else
                        op.Add(t);
                }
            }

            return (n, op);
        }

        private static bool EndsWithHalant(string s, char halant)
            => !string.IsNullOrEmpty(s) && s[^1] == halant;

        private static bool IsSingleChar(string s, out char c)
        {
            c = default;
            if (string.IsNullOrEmpty(s) || s.Length != 1) return false;
            c = s[0];
            return true;
        }

        private static bool IsSingleDependentVowel(string s, AsciiToUnicodeConfig cfg)
            => IsSingleChar(s, out char c) && cfg.DependentVowels.Contains(c);

        private static List<string> ProcessVattakshara(List<string> letters, string t, AsciiToUnicodeConfig cfg)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;
            string baseLetter = cfg.Vattaksharagalu[t];

            if (IsSingleDependentVowel(last, cfg))
            {
                letters[^1] = cfg.Halant.ToString();
                letters.Add(baseLetter);
                letters.Add(last);
            }
            else
            {
                letters.Add(cfg.Halant.ToString());
                letters.Add(baseLetter);
            }

            return letters;
        }

        private static List<string> ProcessArkavattu(List<string> letters, string t, AsciiToUnicodeConfig cfg)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;
            string secondLast = letters.Count > 1 ? letters[^2] : string.Empty;

            string ra = cfg.AsciiArkavattu[t];

            if (IsSingleDependentVowel(last, cfg))
            {
                if (letters.Count >= 2)
                {
                    letters[^2] = ra;
                    letters[^1] = cfg.Halant.ToString();
                    letters.Add(secondLast);
                    letters.Add(last);
                }
                else
                {
                    letters.Add(ra);
                    letters.Add(cfg.Halant.ToString());
                }
            }
            else
            {
                if (letters.Count >= 1)
                {
                    letters[^1] = ra;
                    letters.Add(cfg.Halant.ToString());
                    letters.Add(last);
                }
                else
                {
                    letters.Add(ra);
                    letters.Add(cfg.Halant.ToString());
                }
            }

            return letters;
        }

        private static List<string> ProcessBrokenCase(List<string> letters, string t, AsciiToUnicodeConfig cfg)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;
            var bc = cfg.BrokenCases[t];

            if (IsSingleChar(last, out char lastChar) && bc.Mapping.TryGetValue(lastChar, out char replacement))
                letters[^1] = replacement.ToString();
            else
                letters.Add(bc.Value);

            return letters;
        }

        // ============================================================
        // Postprocess 1: Kannada cluster normalization (conjunct/vowel order)
        // ============================================================
        private static string PostProcessKannadaClusters(string text, AsciiToUnicodeConfig cfg)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new StringBuilder(text.Length);
            int i = 0;

            while (i < text.Length)
            {
                if (!IsKannadaChar(text[i]))
                {
                    sb.Append(text[i]);
                    i++;
                    continue;
                }

                int start = i;
                while (i < text.Length && IsKannadaChar(text[i]))
                    i++;

                string run = text.Substring(start, i - start);
                sb.Append(NormalizeKannadaRun(run, cfg));
            }

            return sb.ToString();
        }

        private static string NormalizeKannadaRun(string run, AsciiToUnicodeConfig cfg)
        {
            var sb = new StringBuilder(run.Length);

            int i = 0;
            while (i < run.Length)
            {
                char ch = run[i];

                if (!IsDependentVowelSign(ch, cfg))
                {
                    sb.Append(ch);
                    i++;
                    continue;
                }

                // Move vowel sign AFTER a (halant + consonant)+ chain when it appears before it.
                if (i + 1 < run.Length && run[i + 1] == cfg.Halant)
                {
                    char vowel = ch;
                    int j = i + 1;

                    var chain = new StringBuilder();
                    while (j < run.Length && run[j] == cfg.Halant)
                    {
                        if (j + 1 >= run.Length) break;
                        char cons = run[j + 1];
                        if (!IsKannadaConsonant(cons)) break;

                        chain.Append(cfg.Halant);
                        chain.Append(cons);
                        j += 2;
                    }

                    sb.Append(chain);
                    sb.Append(vowel);

                    i = j;
                    continue;
                }

                sb.Append(ch);
                i++;
            }

            return sb.ToString();
        }

        private static bool IsKannadaChar(char c)
            => (c >= '\u0C80' && c <= '\u0CFF') || c == '\u200C' || c == '\u200D';

        private static bool IsKannadaConsonant(char c)
            => (c >= '\u0C95' && c <= '\u0CB9') || c == '\u0CDE';

        private static bool IsDependentVowelSign(char c, AsciiToUnicodeConfig cfg)
            => cfg.DependentVowels.Contains(c);

        // ============================================================
        // Postprocess 2: Final fixups (data-driven)
        // ============================================================
        private static string ApplyPostFixups(string txt, AsciiToUnicodeConfig cfg)
        {
            if (string.IsNullOrEmpty(txt)) return txt;
            if (cfg.PostFixups.Count == 0) return txt;

            foreach (var fx in cfg.PostFixups)
            {
                if (!string.IsNullOrEmpty(fx.From))
                    txt = txt.Replace(fx.From, fx.To ?? string.Empty, StringComparison.Ordinal);
            }

            return txt;
        }

        // ============================================================
        // JSON DTO + runtime config
        // ============================================================
        private sealed class AsciiToUnicodeConfig
        {
            public required int MaxTokenLength { get; init; }
            public required char Zwj { get; init; }
            public required char Zwnj { get; init; }
            public required char Halant { get; init; }

            public required char AsciiHalantChar { get; init; }
            public required HashSet<char> AsciiConsonantStartChars { get; init; }

            public required Dictionary<string, string> Mapping { get; init; }
            public required HashSet<char> DependentVowels { get; init; }
            public required HashSet<char> IgnoreList { get; init; }

            public required Dictionary<string, string> Vattaksharagalu { get; init; }
            public required Dictionary<string, string> AsciiArkavattu { get; init; }
            public required Dictionary<string, BrokenCase> BrokenCases { get; init; }

            public required List<PostFixup> PostFixups { get; init; }

            public static AsciiToUnicodeConfig From(AsciiToUnicodeJson root)
            {
                if (root.Meta == null) throw new InvalidOperationException("meta missing.");
                if (root.Mapping == null) throw new InvalidOperationException("mapping missing.");

                int maxToken = root.Meta.MaxTokenLength > 0 ? root.Meta.MaxTokenLength : 4;
                if (maxToken > 8) maxToken = 8;

                char zwj = !string.IsNullOrEmpty(root.Meta.Zwj) ? root.Meta.Zwj[0] : '\u200D';
                char zwnj = '\u200C';
                char halant = !string.IsNullOrEmpty(root.Meta.Halant) ? root.Meta.Halant[0] : '\u0CCD';

                char asciiHalantChar = '\u00EF'; // ï (0xEF) [file:41]

                const string asciiConsonantStartChars =
                    "JRmpL" +
                    "\u00B0\u00AC" +
                    "aej0" +
                    "\u03BC" +
                    "qC" +
                    "\u00A7" +
                    "S" +
                    "\u00AA" +
                    "kv" +
                    "\u00BF" +
                    "y" +
                    "\u00A2\u00BC" +
                    "lAbFU" +
                    "\u00A3" +
                    "E" +
                    "\u00AE\u00BE\u00B1" +
                    "DGKPMQZo" +
                    "\u00B2\u00B9" +
                    "z" +
                    "\u00B6\u00BD\u00A8\u00BB" +
                    "urT" +
                    "\u00A6" +
                    "gndNfwWItHc" +
                    "\u00AB\u00B5" +
                    "O" +
                    "\u00B8" +
                    "VY" +
                    "\u00BA" +
                    "x" +
                    "\u00A5\u00A9" +
                    "X";

                return new AsciiToUnicodeConfig
                {
                    MaxTokenLength = maxToken,
                    Zwj = zwj,
                    Zwnj = zwnj,
                    Halant = halant,

                    AsciiHalantChar = asciiHalantChar,
                    AsciiConsonantStartChars = new HashSet<char>(asciiConsonantStartChars),

                    Mapping = root.Mapping,

                    DependentVowels = new HashSet<char>((root.DependentVowels ?? Array.Empty<string>())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s[0])),

                    IgnoreList = new HashSet<char>((root.IgnoreList ?? Array.Empty<string>())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => s[0])),

                    Vattaksharagalu = root.Vattaksharagalu ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    AsciiArkavattu = root.AsciiArkavattu ?? new Dictionary<string, string>(StringComparer.Ordinal),

                    BrokenCases = (root.BrokenCases ?? new Dictionary<string, BrokenCaseJson>())
                        .ToDictionary(kv => kv.Key, kv => BrokenCase.From(kv.Value), StringComparer.Ordinal),

                    PostFixups = (root.PostFixups ?? new List<PostFixupJson>())
                        .Where(x => x != null)
                        .Select(x => new PostFixup { From = x.From ?? string.Empty, To = x.To ?? string.Empty })
                        .ToList()
                };
            }
        }

        private sealed class PostFixup
        {
            public required string From { get; init; }
            public required string To { get; init; }
        }

        private sealed class BrokenCase
        {
            public required string Value { get; init; }
            public required Dictionary<char, char> Mapping { get; init; }

            public static BrokenCase From(BrokenCaseJson json)
            {
                var map = new Dictionary<char, char>();
                if (json.Mapping != null)
                {
                    foreach (var kv in json.Mapping)
                    {
                        if (!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                            map[kv.Key[0]] = kv.Value[0];
                    }
                }

                return new BrokenCase
                {
                    Value = json.Value ?? string.Empty,
                    Mapping = map
                };
            }
        }

        private sealed class AsciiToUnicodeJson
        {
            public MetaJson? Meta { get; set; }
            public Dictionary<string, string>? Mapping { get; set; }
            public Dictionary<string, BrokenCaseJson>? BrokenCases { get; set; }

            public string[]? DependentVowels { get; set; }
            public string[]? IgnoreList { get; set; }

            public Dictionary<string, string>? Vattaksharagalu { get; set; }
            public Dictionary<string, string>? AsciiArkavattu { get; set; }

            public List<PostFixupJson>? PostFixups { get; set; }
        }

        private sealed class MetaJson
        {
            public int MaxTokenLength { get; set; }
            public string? Zwj { get; set; }
            public string? Halant { get; set; }
        }

        private sealed class BrokenCaseJson
        {
            public string? Value { get; set; }
            public Dictionary<string, string>? Mapping { get; set; }
        }

        private sealed class PostFixupJson
        {
            public string? From { get; set; }
            public string? To { get; set; }
        }
    }
}

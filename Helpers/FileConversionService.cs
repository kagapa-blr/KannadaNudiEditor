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
        // ============================================================
        // Public API (plug into MainWindow)
        // ============================================================
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
                SimpleLogger.Log($"[A2U] Start | chars={input.Length} | maxToken={cfg.MaxTokenLength}");

                //string output = ProcessLineLikePython(input, cfg);
                string output = ProcessParagraphPreserveSpacing(input, cfg);


                SimpleLogger.Log($"[A2U] Done  | chars={output.Length}");
                return output;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[A2U] Failed");
                return input ?? string.Empty;
            }
        }

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


        // Placeholder as requested

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

            var opts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            var root = JsonSerializer.Deserialize<AsciiToUnicodeJson>(json, opts)
                       ?? throw new InvalidOperationException("Failed to deserialize AsciiToUnicodeMapping.json."); // [web:68]

            var cfg = AsciiToUnicodeConfig.From(root);

            SimpleLogger.Log($"[A2U] Mapping loaded | mapping={cfg.Mapping.Count} | vattu={cfg.Vattaksharagalu.Count} | broken={cfg.BrokenCases.Count}");
            return cfg;
        }

        // ============================================================
        // Core conversion (Python-equivalent)
        // ============================================================

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
                    // Python: if previous output endswith halant => insert ZWJ
                    if (op.Count > 0 && EndsWithHalant(op[^1], cfg.Halant))
                        op.Add(cfg.Zwj.ToString());

                    op.Add(mapped);
                    n = i;
                    break;
                }

                if (i > 0) continue;

                // last attempt => special handlers like Python
                if (cfg.AsciiArkavattu.ContainsKey(t))
                    op = ProcessArkavattu(op, t, cfg);
                else if (cfg.Vattaksharagalu.ContainsKey(t))
                    op = ProcessVattakshara(op, t, cfg);
                else if (cfg.BrokenCases.ContainsKey(t))
                    op = ProcessBrokenCase(op, t, cfg);
                else
                    op.Add(t);
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
        // JSON DTO + runtime config
        // ============================================================
        private sealed class AsciiToUnicodeConfig
        {
            public required int MaxTokenLength { get; init; }
            public required char Zwj { get; init; }
            public required char Halant { get; init; }

            public required Dictionary<string, string> Mapping { get; init; }
            public required HashSet<char> DependentVowels { get; init; }
            public required HashSet<char> IgnoreList { get; init; }

            public required Dictionary<string, string> Vattaksharagalu { get; init; }
            public required Dictionary<string, string> AsciiArkavattu { get; init; }
            public required Dictionary<string, BrokenCase> BrokenCases { get; init; }

            public static AsciiToUnicodeConfig From(AsciiToUnicodeJson root)
            {
                if (root.Meta == null) throw new InvalidOperationException("meta missing.");
                if (root.Mapping == null) throw new InvalidOperationException("mapping missing.");

                int maxToken = root.Meta.MaxTokenLength > 0 ? root.Meta.MaxTokenLength : 4;
                if (maxToken > 8) maxToken = 8; // safety; python uses 4

                char zwj = !string.IsNullOrEmpty(root.Meta.Zwj) ? root.Meta.Zwj[0] : '\u200D';
                char halant = !string.IsNullOrEmpty(root.Meta.Halant) ? root.Meta.Halant[0] : '್';

                return new AsciiToUnicodeConfig
                {
                    MaxTokenLength = maxToken,
                    Zwj = zwj,
                    Halant = halant,

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
                        .ToDictionary(kv => kv.Key, kv => BrokenCase.From(kv.Value), StringComparer.Ordinal)
                };
            }
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
    }
}

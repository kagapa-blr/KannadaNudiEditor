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
        // =========================
        // Public API
        // =========================
        public static string AsciiToUnicodeConverter(string input) => Convert(input, Direction.AsciiToUnicode);

        public static string UnicodeToAsciiConverter(string input) => Convert(input, Direction.UnicodeToAscii);

        // =========================
        // Direction + config cache
        // =========================
        private enum Direction { AsciiToUnicode, UnicodeToAscii }

        private static readonly Lazy<ConversionConfig> A2U = new(() => LoadConfig(Direction.AsciiToUnicode));
        private static readonly Lazy<ConversionConfig> U2A = new(() => LoadConfig(Direction.UnicodeToAscii));

        private static ConversionConfig GetCfg(Direction dir) => dir == Direction.AsciiToUnicode ? A2U.Value : U2A.Value;

        // =========================
        // Conversion pipeline
        // =========================
        private static string Convert(string? input, Direction dir)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    SimpleLogger.Log($"[{dir}] Empty input.");
                    return input ?? string.Empty;
                }

                var cfg = GetCfg(dir);

                // Pre normalize only for ASCII->Unicode direction (Unicode input shouldn't be altered like this)
                string stage0 = (dir == Direction.AsciiToUnicode) ? PreNormalizeAscii(input, cfg) : input;

                // Insert ZWNJ only when enabled
                string stage1 = cfg.EnableZwnjInsertion ? PreInsertZwnj(stage0, cfg) : stage0;

                // Core token mapping
                string mapped = ProcessStreamLongestToken(stage1, cfg);

                // Kannada-specific reorder/normalization only for A2U (by config flag)
                string normalized = cfg.EnableKannadaClusterPostProcess
                    ? PostProcessKannadaClusters(mapped, cfg)
                    : mapped;

                // Fixups (run-based)
                string finalText = ApplyPostFixups(normalized, cfg);

                //SimpleLogger.Log($"[{dir}] Done | chars={finalText.Length}");
                return finalText;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"[{dir}] Failed");
                return input ?? string.Empty;
            }
        }

        // =========================
        // JSON config load
        // =========================
        private static ConversionConfig LoadConfig(Direction dir)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = dir == Direction.AsciiToUnicode ? "AsciiToUnicodeMapping.json" : "UnicodeToAsciiMapping.json";
            string jsonPath = Path.Combine(baseDir, "Resources", fileName);

            SimpleLogger.Log($"[{dir}] Loading mapping JSON: {jsonPath}");

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"{fileName} not found.", jsonPath);

            var opts = CreateJsonOptions();

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            var root = JsonSerializer.Deserialize<ConversionJson>(json, opts)
                       ?? throw new InvalidOperationException($"Failed to deserialize {fileName}.");

            var cfg = ConversionConfig.From(root, dir);

            SimpleLogger.Log(
                $"[{dir}] Loaded | mapping={cfg.Mapping.Count} | broken={cfg.BrokenCases.Count} | fixups={cfg.PostFixups.Count} | vattu={cfg.Vattaksharagalu.Count} | arka={cfg.AsciiArkavattu.Count}");

            return cfg;
        }

        private static JsonSerializerOptions CreateJsonOptions()
            => new()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,  // allow comments [web:31]
                AllowTrailingCommas = true                       // allow trailing commas [web:25]
            };

        // =========================
        // ASCII pre-normalization (A2U only)
        // =========================
        private static string PreNormalizeAscii(string input, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // A) Remove whitespace after ASCII halant (ï + ' ' or ï + '\t')
            input = input.Replace($"{cfg.AsciiHalantChar} ", cfg.AsciiHalantChar.ToString(), StringComparison.Ordinal)
                         .Replace($"{cfg.AsciiHalantChar}\t", cfg.AsciiHalantChar.ToString(), StringComparison.Ordinal);

            // B) Remove whitespace BEFORE special join-chars (fixes: "vÀvÀÛ é..." -> "vÀvÀÛé...")
            if (cfg.AsciiJoinCharsBeforeNoSpace.Count > 0)
            {
                var sb = new StringBuilder(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    char ch = input[i];

                    if ((ch == ' ' || ch == '\t') && i + 1 < input.Length)
                    {
                        char next = input[i + 1];
                        if (cfg.AsciiJoinCharsBeforeNoSpace.Contains(next))
                            continue; // drop this whitespace
                    }

                    sb.Append(ch);
                }
                input = sb.ToString();
            }

            return input;
        }

        // =========================
        // ZWNJ insertion (A2U)
        // =========================
        private static string PreInsertZwnj(string input, ConversionConfig cfg)
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

        // =========================
        // Core mapping: streaming longest token match
        // =========================
        private static string ProcessStreamLongestToken(string text, ConversionConfig cfg)
        {
            if (text == null) return string.Empty;

            int i = 0;
            int maxLen = text.Length;
            var op = new List<string>(capacity: Math.Min(256, maxLen * 2));

            while (i < maxLen)
            {
                if (cfg.IgnoreList.Contains(text[i]))
                {
                    i++;
                    continue;
                }

                int jump = FindAndAppend(op, text, i, cfg);
                i += 1 + jump;
            }

            return string.Concat(op);
        }

        private static int FindAndAppend(List<string> op, string txt, int pos, ConversionConfig cfg)
        {
            int remaining = txt.Length - pos;
            int maxLen = Math.Min(cfg.MaxTokenLength, remaining - 1);
            if (maxLen < 0) maxLen = 0;

            for (int i = maxLen; i >= 0; i--)
            {
                string t = txt.Substring(pos, i + 1);

                if (cfg.Mapping.TryGetValue(t, out string mapped))
                {
                    if (cfg.EnableZwjAfterHalant && op.Count > 0 && EndsWith(op[^1], cfg.Halant))
                        op.Add(cfg.Zwj.ToString());

                    op.Add(mapped);
                    return i;
                }

                if (i > 0) continue;

                // fallback handlers
                if (cfg.AsciiArkavattu.TryGetValue(t, out var ra))
                    ProcessArkavattu(op, ra, cfg);
                else if (cfg.Vattaksharagalu.TryGetValue(t, out var baseLetter))
                    ProcessVattakshara(op, baseLetter, cfg);
                else if (cfg.BrokenCases.TryGetValue(t, out var bc))
                    ProcessBrokenCase(op, bc);
                else
                {
                    if (t.Length == 1 && t[0] == cfg.AsciiHalantChar)
                        op.Add(cfg.Halant.ToString());
                    else
                        op.Add(t);
                }

                return 0;
            }

            return 0;
        }

        private static bool EndsWith(string s, char c) => !string.IsNullOrEmpty(s) && s[^1] == c;

        private static bool IsSingleChar(string s, out char c)
        {
            c = default;
            if (string.IsNullOrEmpty(s) || s.Length != 1) return false;
            c = s[0];
            return true;
        }

        private static bool IsSingleDependentVowel(string s, ConversionConfig cfg)
            => IsSingleChar(s, out char c) && cfg.DependentVowels.Contains(c);

        // =========================
        // Fallback handlers
        // =========================
        private static void ProcessVattakshara(List<string> letters, string baseLetter, ConversionConfig cfg)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;

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
        }

        private static void ProcessArkavattu(List<string> letters, string raLetter, ConversionConfig cfg)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;
            string secondLast = letters.Count > 1 ? letters[^2] : string.Empty;

            if (IsSingleDependentVowel(last, cfg))
            {
                if (letters.Count >= 2)
                {
                    letters[^2] = raLetter;
                    letters[^1] = cfg.Halant.ToString();
                    letters.Add(secondLast);
                    letters.Add(last);
                }
                else
                {
                    letters.Add(raLetter);
                    letters.Add(cfg.Halant.ToString());
                }
            }
            else
            {
                if (letters.Count >= 1)
                {
                    letters[^1] = raLetter;
                    letters.Add(cfg.Halant.ToString());
                    letters.Add(last);
                }
                else
                {
                    letters.Add(raLetter);
                    letters.Add(cfg.Halant.ToString());
                }
            }
        }

        private static void ProcessBrokenCase(List<string> letters, BrokenCase bc)
        {
            string last = letters.Count > 0 ? letters[^1] : string.Empty;

            if (IsSingleChar(last, out char lastChar) && bc.Mapping.TryGetValue(lastChar, out char replacement))
                letters[^1] = replacement.ToString();
            else
                letters.Add(bc.Value);
        }

        // =========================
        // Postprocess 1: Kannada run normalization (A2U)
        // =========================
        private static string PostProcessKannadaClusters(string text, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new StringBuilder(text.Length);
            int i = 0;

            while (i < text.Length)
            {
                if (!IsKannadaOrJoiner(text[i]))
                {
                    sb.Append(text[i]);
                    i++;
                    continue;
                }

                int start = i;
                while (i < text.Length && IsKannadaOrJoiner(text[i]))
                    i++;

                string run = text.Substring(start, i - start);
                sb.Append(NormalizeKannadaRun(run, cfg));
            }

            return sb.ToString();
        }

        private static string NormalizeKannadaRun(string run, ConversionConfig cfg)
        {
            var sb = new StringBuilder(run.Length);
            int i = 0;

            while (i < run.Length)
            {
                char ch = run[i];

                if (!cfg.DependentVowels.Contains(ch))
                {
                    sb.Append(ch);
                    i++;
                    continue;
                }

                // Move vowel sign after (halant+consonant)+ chain when it incorrectly appears before it
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

        private static bool IsKannadaConsonant(char c)
            => (c >= '\u0C95' && c <= '\u0CB9') || c == '\u0CDE';

        // Kannada + ZWJ/ZWNJ run detection [web:114]
        private static bool IsKannadaOrJoiner(char c)
            => (c >= '\u0C80' && c <= '\u0CFF') || c == '\u200C' || c == '\u200D';

        // =========================
        // Postprocess 2: fixups (run-based)
        // =========================
        private static string ApplyPostFixups(string txt, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(txt)) return txt;
            if (cfg.PostFixups.Count == 0) return txt;

            var sb = new StringBuilder(txt.Length);
            int i = 0;

            while (i < txt.Length)
            {
                if (!IsKannadaOrJoiner(txt[i]))
                {
                    sb.Append(txt[i]);
                    i++;
                    continue;
                }

                int start = i;
                while (i < txt.Length && IsKannadaOrJoiner(txt[i]))
                    i++;

                string run = txt.Substring(start, i - start);

                foreach (var fx in cfg.PostFixups)
                {
                    if (!string.IsNullOrEmpty(fx.From))
                        run = run.Replace(fx.From, fx.To ?? string.Empty, StringComparison.Ordinal);
                }

                sb.Append(run);
            }

            return sb.ToString();
        }

        // =========================
        // DTOs + runtime config
        // =========================
        private sealed class ConversionConfig
        {
            public required int MaxTokenLength { get; init; }
            public required char Zwj { get; init; }
            public required char Zwnj { get; init; }
            public required char Halant { get; init; }

            public required char AsciiHalantChar { get; init; }
            public required HashSet<char> AsciiConsonantStartChars { get; init; }
            public required HashSet<char> AsciiJoinCharsBeforeNoSpace { get; init; }

            public required Dictionary<string, string> Mapping { get; init; }
            public required HashSet<char> DependentVowels { get; init; }
            public required HashSet<char> IgnoreList { get; init; }

            public required Dictionary<string, string> Vattaksharagalu { get; init; }
            public required Dictionary<string, string> AsciiArkavattu { get; init; }
            public required Dictionary<string, BrokenCase> BrokenCases { get; init; }
            public required List<PostFixup> PostFixups { get; init; }

            public required bool EnableZwnjInsertion { get; init; }
            public required bool EnableZwjAfterHalant { get; init; }
            public required bool EnableKannadaClusterPostProcess { get; init; }

            public static ConversionConfig From(ConversionJson root, Direction dir)
            {
                if (root.Meta == null) throw new InvalidOperationException("meta missing.");
                if (root.Mapping == null) throw new InvalidOperationException("mapping missing.");

                int maxToken = root.Meta.MaxTokenLength > 0 ? root.Meta.MaxTokenLength : 4;
                if (maxToken > 16) maxToken = 16;

                char zwj = FirstOrDefault(root.Meta.Zwj, '\u200D');
                char zwnj = FirstOrDefault(root.Meta.Zwnj, '\u200C');
                char halant = FirstOrDefault(root.Meta.Halant, '\u0CCD');
                char asciiHalantChar = FirstOrDefault(root.Meta.AsciiHalantChar, '\u00EF');

                string consonantStarts = root.AsciiConsonantStartChars ?? string.Empty;
                string joinBeforeNoSpace = root.AsciiJoinCharsBeforeNoSpace ?? string.Empty;

                return new ConversionConfig
                {
                    MaxTokenLength = maxToken,
                    Zwj = zwj,
                    Zwnj = zwnj,
                    Halant = halant,

                    AsciiHalantChar = asciiHalantChar,
                    AsciiConsonantStartChars = new HashSet<char>(consonantStarts),
                    AsciiJoinCharsBeforeNoSpace = new HashSet<char>(joinBeforeNoSpace),

                    Mapping = root.Mapping, // keep key exactness (don’t ignore case)
                    DependentVowels = ToCharSet(root.DependentVowels),
                    IgnoreList = ToCharSet(root.IgnoreList),

                    Vattaksharagalu = root.Vattaksharagalu ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    AsciiArkavattu = root.AsciiArkavattu ?? new Dictionary<string, string>(StringComparer.Ordinal),

                    BrokenCases = (root.BrokenCases ?? new Dictionary<string, BrokenCaseJson>())
                        .ToDictionary(kv => kv.Key, kv => BrokenCase.From(kv.Value), StringComparer.Ordinal),

                    PostFixups = (root.PostFixups ?? new List<PostFixupJson>())
                        .Where(x => x != null)
                        .Select(x => new PostFixup { From = x!.From ?? string.Empty, To = x!.To ?? string.Empty })
                        .ToList(),

                    EnableZwnjInsertion = dir == Direction.AsciiToUnicode,
                    EnableZwjAfterHalant = dir == Direction.AsciiToUnicode,
                    EnableKannadaClusterPostProcess = dir == Direction.AsciiToUnicode
                };
            }

            private static char FirstOrDefault(string? s, char fallback)
                => !string.IsNullOrEmpty(s) ? s[0] : fallback;

            private static HashSet<char> ToCharSet(string[]? arr)
                => new((arr ?? Array.Empty<string>())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s[0]));
        }

        private sealed class ConversionJson
        {
            public MetaJson? Meta { get; set; }

            public string? AsciiConsonantStartChars { get; set; }
            public string? AsciiJoinCharsBeforeNoSpace { get; set; }

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
            public string? Zwnj { get; set; }
            public string? Halant { get; set; }
            public string? AsciiHalantChar { get; set; }
        }

        private sealed class PostFixup
        {
            public required string From { get; init; }
            public required string To { get; init; }
        }

        private sealed class PostFixupJson
        {
            public string? From { get; set; }
            public string? To { get; set; }
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

        private sealed class BrokenCaseJson
        {
            public string? Value { get; set; }
            public Dictionary<string, string>? Mapping { get; set; }
        }
    }
}

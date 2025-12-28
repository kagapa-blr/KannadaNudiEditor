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
        public static string AsciiToUnicodeConverter(string input) => Convert(input, Direction.AsciiToUnicode);
        public static string UnicodeToAsciiConverter(string input) => Convert(input, Direction.UnicodeToAscii);

        private enum Direction { AsciiToUnicode, UnicodeToAscii }

        private static readonly Lazy<ConversionConfig> A2U = new(() => LoadConfig(Direction.AsciiToUnicode));
        private static readonly Lazy<ConversionConfig> U2A = new(() => LoadConfig(Direction.UnicodeToAscii));

        private static ConversionConfig GetCfg(Direction dir) => dir == Direction.AsciiToUnicode ? A2U.Value : U2A.Value;

        private static string Convert(string? input, Direction dir)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return input ?? string.Empty;

                var cfg = GetCfg(dir);

                string stage0 = input;

                if (dir == Direction.AsciiToUnicode)
                {
                    stage0 = FileConversionUtilities.PreNormalizeAscii(stage0, cfg.AsciiHalantChar, cfg.AsciiJoinCharsBeforeNoSpace);
                    if (cfg.EnableZwnjInsertion)
                        stage0 = FileConversionUtilities.PreInsertZwnj(stage0, cfg.AsciiHalantChar, cfg.AsciiConsonantStartChars, cfg.Zwnj);
                }

                string mapped = ProcessStreamLongestToken(stage0, cfg);

                if (cfg.EnableKannadaClusterPostProcess)
                    mapped = FileConversionUtilities.PostProcessKannadaClusters(mapped, cfg.DependentVowels, cfg.Halant);

                string finalText = FileConversionUtilities.ApplyPostFixupsKannadaRuns(mapped, cfg.PostFixupsPairs);
                return finalText;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"[{dir}] Failed");
                return input ?? string.Empty;
            }
        }

        private static ConversionConfig LoadConfig(Direction dir)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = dir == Direction.AsciiToUnicode ? "AsciiToUnicodeMapping.json" : "UnicodeToAsciiMapping.json";
            string jsonPath = Path.Combine(baseDir, "Resources", fileName);

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"{fileName} not found.", jsonPath);

            var opts = CreateJsonOptions();

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);
            var root = JsonSerializer.Deserialize<ConversionJson>(json, opts)
                       ?? throw new InvalidOperationException($"Failed to deserialize {fileName}.");

            return ConversionConfig.From(root, dir);
        }

        private static JsonSerializerOptions CreateJsonOptions()
            => new()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip, // [web:31]
                AllowTrailingCommas = true // [web:25]
            };

        // ---- core tokenizer (kept here; depends on cfg mapping dictionaries) ----

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
                    if (cfg.EnableZwjAfterHalant && op.Count > 0 && FileConversionUtilities.EndsWith(op[^1], cfg.Halant))
                        op.Add(cfg.Zwj.ToString());

                    op.Add(mapped);
                    return i;
                }

                if (i > 0) continue;

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

        private static bool IsSingleDependentVowel(string s, ConversionConfig cfg)
            => FileConversionUtilities.IsSingleChar(s, out char c) && cfg.DependentVowels.Contains(c);

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

            if (FileConversionUtilities.IsSingleChar(last, out char lastChar) && bc.Mapping.TryGetValue(lastChar, out char replacement))
                letters[^1] = replacement.ToString();
            else
                letters.Add(bc.Value);
        }

        // ---- DTOs ----

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
            public required List<(string From, string To)> PostFixupsPairs { get; init; }

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

                var postFixups = (root.PostFixups ?? new List<PostFixupJson>())
                    .Where(x => x != null)
                    .Select(x => new PostFixup { From = x!.From ?? string.Empty, To = x!.To ?? string.Empty })
                    .ToList();

                return new ConversionConfig
                {
                    MaxTokenLength = maxToken,
                    Zwj = zwj,
                    Zwnj = zwnj,
                    Halant = halant,

                    AsciiHalantChar = asciiHalantChar,
                    AsciiConsonantStartChars = new HashSet<char>(root.AsciiConsonantStartChars ?? string.Empty),
                    AsciiJoinCharsBeforeNoSpace = new HashSet<char>(root.AsciiJoinCharsBeforeNoSpace ?? string.Empty),

                    Mapping = root.Mapping,
                    DependentVowels = ToCharSet(root.DependentVowels),
                    IgnoreList = ToCharSet(root.IgnoreList),

                    Vattaksharagalu = root.Vattaksharagalu ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    AsciiArkavattu = root.AsciiArkavattu ?? new Dictionary<string, string>(StringComparer.Ordinal),
                    BrokenCases = (root.BrokenCases ?? new Dictionary<string, BrokenCaseJson>())
                        .ToDictionary(kv => kv.Key, kv => BrokenCase.From(kv.Value), StringComparer.Ordinal),

                    PostFixups = postFixups,
                    PostFixupsPairs = postFixups.Select(p => (p.From, p.To)).ToList(),

                    EnableZwnjInsertion = dir == Direction.AsciiToUnicode,
                    EnableZwjAfterHalant = dir == Direction.AsciiToUnicode,
                    EnableKannadaClusterPostProcess = dir == Direction.AsciiToUnicode
                };
            }

            private static char FirstOrDefault(string? s, char fallback) => !string.IsNullOrEmpty(s) ? s[0] : fallback;

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

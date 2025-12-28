using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KannadaNudiEditor.Helpers
{
    internal static class FileConversionUtilities
    {
        // =========================================================
        // Public conversion entry points (called by FileConversionService)
        // =========================================================
        public static string ConvertAsciiToUnicode(string input, ConversionConfig cfg)
        {
            string stage0 = PreNormalizeAscii(input, cfg.AsciiHalantChar, cfg.AsciiJoinCharsBeforeNoSpace);

            if (cfg.EnableZwnjInsertion)
                stage0 = PreInsertZwnj(stage0, cfg.AsciiHalantChar, cfg.AsciiConsonantStartChars, cfg.Zwnj);

            string mapped = ProcessStreamLongestToken(stage0, cfg);

            if (cfg.EnableKannadaClusterPostProcess)
                mapped = PostProcessKannadaClusters(mapped, cfg.DependentVowels, cfg.Halant);

            return ApplyPostFixupsKannadaRuns(mapped, cfg.PostFixupsPairs);
        }

        public static string ConvertUnicodeToAscii(string input, ConversionConfig cfg)
        {
            // JS behavior: split on spaces and convert each word. [file:147]
            var words = input.Split(' ', StringSplitOptions.None);
            var sb = new StringBuilder(input.Length * 2);

            for (int wi = 0; wi < words.Length; wi++)
            {
                if (wi > 0) sb.Append(' ');

                string w = words[wi];
                if (string.IsNullOrEmpty(w))
                    continue;

                sb.Append(ConvertUnicodeWordToAscii(w, cfg));
            }

            return sb.ToString();
        }

        // =========================================================
        // Unicode -> ASCII core (cluster + rules + mapping)
        // =========================================================
        private static string ConvertUnicodeWordToAscii(string word, ConversionConfig cfg)
        {
            var letters = SplitKannadaLetters(word, cfg.UnicodePrevValueChars);

            var outp = new StringBuilder(word.Length * 2);
            foreach (var letter in letters)
                outp.Append(RearrangeAndReplace(letter, cfg));

            string s = RemoveZwnj(outp.ToString()); // JS removes ZWNJ. [file:147]

            if (cfg.KannadaDigitToAscii.Count > 0)
                s = ReplaceByCharMap(s, cfg.KannadaDigitToAscii);

            return s;
        }

        // Port-like structure of JS rearrangeandreplace(). [file:147]
        private static string RearrangeAndReplace(string inp, ConversionConfig cfg)
        {
            if (cfg.Mapping.TryGetValue(inp, out var direct))
                return direct;

            if (cfg.UnicodeVowels.Contains(inp))
                return MapOrSelf(inp, cfg);

            // vowel + anusvara/visarga
            inp = cfg.RegexUniVowelPlusAnusvaraVisarga.Replace(inp, m =>
            {
                string v = m.Groups["v"].Value;
                string av = m.Groups["av"].Value;
                return MapOrSelf(v, cfg) + MapOrSelf(av, cfg);
            });

            // consonant + dependent vowel(s)
            inp = cfg.RegexUniConsonantPlusVowel.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string dv = m.Groups["dv"].Value;
                return SubstituteAscii(b, dv, vattaChain: "", appendChars: null, cfg);
            });

            // vattakshara chain (ignore ZWJ in chain)
            inp = cfg.RegexUniVattakshara.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string chain = m.Groups["chain"].Value;
                string dv = m.Groups["dv"].Value;
                return SubstituteAscii(b, dv, chain, appendChars: null, cfg);
            });

            // reph-without-ZWJ => arkavattu behavior (append asciiArkavattuChar)
            // NOTE: This must be aligned with your exact JS regex groups for full correctness. [file:147]
            inp = cfg.RegexUniRephWithoutZwj.Replace(inp, m =>
            {
                string firstVattaCons = m.Groups["firstVattaCons"].Value; // consonant
                string restChain = m.Groups["restChain"].Value;           // halant+cons repeats
                string dv = m.Groups["dv"].Value;

                return SubstituteAscii(firstVattaCons, dv, restChain, appendChars: cfg.AsciiArkavattuChar.ToString(), cfg);
            });

            return MapOrSelf(inp, cfg);
        }

        private static string SubstituteAscii(string baseCons, string depVowels, string vattaChain, string? appendChars, ConversionConfig cfg)
        {
            var sb = new StringBuilder();

            char? dep0 = depVowels.Length > 0 ? depVowels[0] : null;
            char? dep1 = depVowels.Length > 1 ? depVowels[1] : null;

            bool dep0IsAnusOrVis = dep0.HasValue && (dep0.Value == '\u0C82' || dep0.Value == '\u0C83');

            if (!dep0.HasValue)
            {
                sb.Append(MapOrSelf(baseCons, cfg));
            }
            else if (!dep0IsAnusOrVis)
            {
                sb.Append(MapOrSelf(baseCons + dep0.Value, cfg));
            }
            else
            {
                sb.Append(MapOrSelf(baseCons, cfg));
            }

            // append vatta consonants (each after halant)
            if (!string.IsNullOrEmpty(vattaChain))
            {
                for (int i = 0; i < vattaChain.Length; i++)
                {
                    char ch = vattaChain[i];
                    if (ch == cfg.Zwj) continue;

                    if (ch == cfg.Halant && i + 1 < vattaChain.Length)
                    {
                        char cons = vattaChain[i + 1];
                        sb.Append(MapOrSelf(cons.ToString(), cfg));
                        i++;
                    }
                }
            }

            if (dep0IsAnusOrVis)
                sb.Append(MapOrSelf(dep0.Value.ToString(), cfg));

            if (dep1.HasValue)
                sb.Append(MapOrSelf(dep1.Value.ToString(), cfg));

            if (!string.IsNullOrEmpty(appendChars))
                sb.Append(appendChars);

            return sb.ToString();
        }

        private static string MapOrSelf(string s, ConversionConfig cfg)
            => cfg.Mapping.TryGetValue(s, out var mapped) ? mapped : s;

        private static string ReplaceByCharMap(string s, Dictionary<char, char> map)
        {
            if (string.IsNullOrEmpty(s) || map.Count == 0) return s;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                sb.Append(map.TryGetValue(ch, out var rep) ? rep : ch);
            return sb.ToString();
        }

        // =========================================================
        // A2U core: streaming longest-token match (same behavior as before)
        // =========================================================
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
            => IsSingleChar(s, out char c) && cfg.DependentVowels.Contains(c);

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

        // =========================================================
        // Shared / existing utilities (mostly your existing code)
        // =========================================================
        public static bool IsKannadaOrJoiner(char c)
            => (c >= '\u0C80' && c <= '\u0CFF') || c == '\u200C' || c == '\u200D';

        public static bool IsKannadaConsonant(char c)
            => (c >= '\u0C95' && c <= '\u0CB9') || c == '\u0CDE';

        public static bool EndsWith(string s, char c)
            => !string.IsNullOrEmpty(s) && s[^1] == c;

        public static bool IsSingleChar(string s, out char c)
        {
            c = default;
            if (string.IsNullOrEmpty(s) || s.Length != 1) return false;
            c = s[0];
            return true;
        }

        public static string PreNormalizeAscii(string input, char asciiHalantChar, HashSet<char> joinCharsBeforeNoSpace)
        {
            if (string.IsNullOrEmpty(input)) return input;

            input = input.Replace($"{asciiHalantChar} ", asciiHalantChar.ToString(), StringComparison.Ordinal)
                         .Replace($"{asciiHalantChar}\t", asciiHalantChar.ToString(), StringComparison.Ordinal);

            if (joinCharsBeforeNoSpace != null && joinCharsBeforeNoSpace.Count > 0)
            {
                var sb = new StringBuilder(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    char ch = input[i];

                    if ((ch == ' ' || ch == '\t') && i + 1 < input.Length)
                    {
                        char next = input[i + 1];
                        if (joinCharsBeforeNoSpace.Contains(next))
                            continue;
                    }

                    sb.Append(ch);
                }
                input = sb.ToString();
            }

            return input;
        }

        public static string PreInsertZwnj(string input, char asciiHalantChar, HashSet<char> asciiConsonantStartChars, char zwnj)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.IndexOf(asciiHalantChar) < 0) return input;

            var sb = new StringBuilder(input.Length + 16);

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                sb.Append(ch);

                if (ch == asciiHalantChar && i + 1 < input.Length)
                {
                    char next = input[i + 1];
                    if (asciiConsonantStartChars.Contains(next))
                        sb.Append(zwnj);
                }
            }

            return sb.ToString();
        }

        public static string ApplyPostFixupsKannadaRuns(string txt, IReadOnlyList<(string From, string To)> fixups)
        {
            if (string.IsNullOrEmpty(txt)) return txt;
            if (fixups == null || fixups.Count == 0) return txt;

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

                foreach (var (from, to) in fixups)
                {
                    if (!string.IsNullOrEmpty(from))
                        run = run.Replace(from, to ?? string.Empty, StringComparison.Ordinal);
                }

                sb.Append(run);
            }

            return sb.ToString();
        }

        public static string ApplyPostFixupsKannadaRuns(string txt, List<(string From, string To)> fixups)
            => ApplyPostFixupsKannadaRuns(txt, (IReadOnlyList<(string From, string To)>)fixups);

        public static string PostProcessKannadaClusters(string text, HashSet<char> dependentVowels, char halant)
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
                sb.Append(NormalizeKannadaRun(run, dependentVowels, halant));
            }

            return sb.ToString();
        }

        private static string NormalizeKannadaRun(string run, HashSet<char> dependentVowels, char halant)
        {
            var sb = new StringBuilder(run.Length);
            int i = 0;

            while (i < run.Length)
            {
                char ch = run[i];

                if (!dependentVowels.Contains(ch))
                {
                    sb.Append(ch);
                    i++;
                    continue;
                }

                if (i + 1 < run.Length && run[i + 1] == halant)
                {
                    char vowel = ch;
                    int j = i + 1;

                    var chain = new StringBuilder();
                    while (j < run.Length && run[j] == halant)
                    {
                        if (j + 1 >= run.Length) break;

                        char cons = run[j + 1];
                        if (!IsKannadaConsonant(cons)) break;

                        chain.Append(halant);
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

        // =========================================================
        // U2A: cluster splitting + ZWNJ removal
        // =========================================================
        public static readonly HashSet<char> DefaultPrevValueChars = new()
        {
            '\u0CBE','\u0CBF','\u0CC0','\u0CC1','\u0CC2','\u0CC3','\u0CC4',
            '\u0CC6','\u0CC7','\u0CC8','\u0CCA','\u0CCB','\u0CCC',
            '\u0CCD', '\u0C82','\u0C83', '\u200D'
        };

        public static List<string> SplitKannadaLetters(string txt, HashSet<char>? prevValueChars)
        {
            prevValueChars ??= DefaultPrevValueChars;

            var outp = new List<string>();
            if (string.IsNullOrEmpty(txt)) return outp;

            for (int i = 0; i < txt.Length; i++)
            {
                char c = txt[i];
                bool attach = prevValueChars.Contains(c);

                if (outp.Count > 0 && IsKannadaOrJoiner(c) && attach)
                    outp[^1] += c;
                else
                    outp.Add(c.ToString());
            }

            return outp;
        }

        public static string RemoveZwnj(string txt)
            => string.IsNullOrEmpty(txt) ? txt : txt.Replace("\u200C", "", StringComparison.Ordinal);

        // =========================================================
        // JSON DTOs + runtime config (moved here)
        // =========================================================
        internal sealed class ConversionConfig
        {
            // shared
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

            public required List<(string From, string To)> PostFixupsPairs { get; init; }

            // toggles
            public required bool EnableZwnjInsertion { get; init; }
            public required bool EnableZwjAfterHalant { get; init; }
            public required bool EnableKannadaClusterPostProcess { get; init; }

            // U2A extras
            public required char AsciiArkavattuChar { get; init; }
            public required HashSet<char> UnicodePrevValueChars { get; init; }
            public required HashSet<string> UnicodeVowels { get; init; }
            public required Dictionary<char, char> KannadaDigitToAscii { get; init; }

            public required Regex RegexUniVowelPlusAnusvaraVisarga { get; init; }
            public required Regex RegexUniConsonantPlusVowel { get; init; }
            public required Regex RegexUniVattakshara { get; init; }
            public required Regex RegexUniRephWithoutZwj { get; init; }

            public static ConversionConfig From(ConversionJson root, bool isAsciiToUnicode)
            {
                if (root.Meta == null) throw new InvalidOperationException("meta missing.");
                if (root.Mapping == null) throw new InvalidOperationException("mapping missing.");

                int maxToken = root.Meta.MaxTokenLength > 0 ? root.Meta.MaxTokenLength : 4;
                if (maxToken > 16) maxToken = 16;

                char zwj = FirstOrDefault(root.Meta.Zwj, '\u200D');
                char zwnj = FirstOrDefault(root.Meta.Zwnj, '\u200C');
                char halant = FirstOrDefault(root.Meta.Halant, '\u0CCD');
                char asciiHalantChar = FirstOrDefault(root.Meta.AsciiHalantChar, '\u00EF');

                // PostFixupsPairs
                var postFixupsPairs = (root.PostFixups ?? new List<PostFixupJson>())
                    .Where(x => x != null)
                    .Select(x => (From: x!.From ?? string.Empty, To: x!.To ?? string.Empty))
                    .ToList();

                // U2A defaults
                char asciiArkavattu = FirstOrDefault(root.Meta.AsciiArkavattuChar, '\u00F0');

                var prevChars = new HashSet<char>((root.UnicodePrevValueChars ?? Array.Empty<string>())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s[0]));
                if (prevChars.Count == 0)
                    prevChars = new HashSet<char>(DefaultPrevValueChars);

                var vowels = new HashSet<string>(root.UnicodeVowels ?? Array.Empty<string>(), StringComparer.Ordinal);

                var digitMap = new Dictionary<char, char>();
                if (root.KannadaDigits != null && root.AsciiDigits != null && root.KannadaDigits.Length == root.AsciiDigits.Length)
                {
                    for (int i = 0; i < root.KannadaDigits.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(root.KannadaDigits[i]) && !string.IsNullOrEmpty(root.AsciiDigits[i]))
                            digitMap[root.KannadaDigits[i][0]] = root.AsciiDigits[i][0];
                    }
                }

                string p1 = root.U2aRegex?.UniVowelPlusAnusvaraVisarga ?? @"(?<v>[\u0C85-\u0C94\u0C8E-\u0C90\u0CE0])(?<av>[\u0C82\u0C83])";
                string p2 = root.U2aRegex?.UniConsonantPlusVowel ?? @"(?<base>[\u0C95-\u0CB9\u0CDE])(?<dv>[\u0CBE-\u0CCC\u0C82\u0C83]{1,2})";
                string p3 = root.U2aRegex?.UniVattakshara ?? @"(?<base>[\u0C95-\u0CB9\u0CDE])(?<chain>(?:\u0CCD\u200D?[\u0C95-\u0CB9\u0CDE])+)(?<dv>[\u0CBE-\u0CCC\u0C82\u0C83]{0,2})";
                string p4 = root.U2aRegex?.UniRephWithoutZwj ?? @"(?<stub>^\b$)"; // placeholder; replace with JS regex.

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

                    PostFixupsPairs = postFixupsPairs,

                    EnableZwnjInsertion = isAsciiToUnicode,
                    EnableZwjAfterHalant = isAsciiToUnicode,
                    EnableKannadaClusterPostProcess = isAsciiToUnicode,

                    AsciiArkavattuChar = asciiArkavattu,
                    UnicodePrevValueChars = prevChars,
                    UnicodeVowels = vowels,
                    KannadaDigitToAscii = digitMap,

                    RegexUniVowelPlusAnusvaraVisarga = new Regex(p1, RegexOptions.Compiled),
                    RegexUniConsonantPlusVowel = new Regex(p2, RegexOptions.Compiled),
                    RegexUniVattakshara = new Regex(p3, RegexOptions.Compiled),
                    RegexUniRephWithoutZwj = new Regex(p4, RegexOptions.Compiled),
                };
            }

            private static char FirstOrDefault(string? s, char fallback) => !string.IsNullOrEmpty(s) ? s[0] : fallback;

            private static HashSet<char> ToCharSet(string[]? arr)
                => new((arr ?? Array.Empty<string>()).Where(s => !string.IsNullOrEmpty(s)).Select(s => s[0]));
        }

        internal sealed class ConversionJson
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

            // U2A
            public string[]? UnicodePrevValueChars { get; set; }
            public string[]? UnicodeVowels { get; set; }
            public string[]? KannadaDigits { get; set; }
            public string[]? AsciiDigits { get; set; }
            public U2aRegexJson? U2aRegex { get; set; }
        }

        internal sealed class U2aRegexJson
        {
            public string? UniVowelPlusAnusvaraVisarga { get; set; }
            public string? UniConsonantPlusVowel { get; set; }
            public string? UniVattakshara { get; set; }
            public string? UniRephWithoutZwj { get; set; }
        }

        internal sealed class MetaJson
        {
            public int MaxTokenLength { get; set; }
            public string? Zwj { get; set; }
            public string? Zwnj { get; set; }
            public string? Halant { get; set; }
            public string? AsciiHalantChar { get; set; }

            // U2A
            public string? AsciiArkavattuChar { get; set; }
        }

        internal sealed class PostFixupJson
        {
            public string? From { get; set; }
            public string? To { get; set; }
        }

        internal sealed class BrokenCase
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

        internal sealed class BrokenCaseJson
        {
            public string? Value { get; set; }
            public Dictionary<string, string>? Mapping { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace KannadaNudiEditor.Helpers
{
    internal static partial class FileConversionUtilities
    {
        // =========================================================
        // Public conversion entry point (A2U)
        // =========================================================
        public static string ConvertAsciiToUnicode(string input, ConversionConfig cfg)
        {
            string stage0 = PreNormalizeAscii(input, cfg.AsciiHalantChar, cfg.AsciiJoinCharsBeforeNoSpace);

            if (cfg.EnableZwnjInsertion)
                stage0 = PreInsertZwnj(stage0, cfg.AsciiHalantChar, cfg.AsciiConsonantStartChars, cfg.Zwnj);

            string mapped = ProcessStreamLongestToken(stage0, cfg);

            if (cfg.EnableKannadaClusterPostProcess)
                mapped = PostProcessKannadaClusters(mapped, cfg.DependentVowels, cfg.Halant);

            mapped = ApplyPostFixupsKannadaRuns(mapped, cfg.PostFixupsPairs);

            // FIX: stabilize combining-mark order/composition
            return mapped.Normalize(NormalizationForm.FormC);
        }


        // =========================================================
        // A2U: streaming longest-token match
        // =========================================================
        private static string ProcessStreamLongestToken(string text, ConversionConfig cfg)
        {
            if (text == null) return string.Empty;

            int i = 0;
            int maxLen = text.Length;
            var op = new List<string>(capacity: Math.Min(256, maxLen * 2));

            while (i < maxLen)
            {
                // FIX: Don't delete ignored chars. Keep them to preserve adjacency.
                if (cfg.IgnoreList.Contains(text[i]))
                {
                    op.Add(text[i].ToString());
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
                    ProcessBrokenCase(op, bc, cfg);
                else
                {
                    if (t.Length == 1 && cfg.AsciiDigitToKannada.TryGetValue(t[0], out var kd))
                        op.Add(kd);
                    else if (t.Length == 1 && t[0] == cfg.AsciiHalantChar)
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

        private static void ProcessBrokenCase(List<string> letters, BrokenCase bc, ConversionConfig cfg)
        {
            if (letters.Count == 0)
                return; // drop modifier if nothing to modify

            string last = letters[^1];

            // Only allow modifier to act on a single dependent vowel sign.
            if (IsSingleChar(last, out char lastChar) &&
                cfg.DependentVowels.Contains(lastChar) &&
                bc.Mapping.TryGetValue(lastChar, out char replacement))
            {
                letters[^1] = replacement.ToString();
                return;
            }

            // Otherwise: drop it (prevents stray "ೀ" / "ು" etc showing up)
            // If you want debugging, you can instead: letters.Add(bc.Value);
        }


        // =========================================================
        // A2U: pre-normalization / join rules
        // =========================================================
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

        public static string PreInsertZwnj(
            string input,
            char asciiHalantChar,
            HashSet<char> asciiConsonantStartChars,
            char zwnj)
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

                    // NEW: don't inject ZWNJ before plain ASCII letters/digits
                    if (next <= 0x007F && (char.IsLetterOrDigit(next) || next == '_' || next == '-'))
                        continue;

                    if (asciiConsonantStartChars.Contains(next))
                        sb.Append(zwnj);
                }
            }

            return sb.ToString();
        }

        // =========================================================
        // A2U: Kannada-run post processing
        // =========================================================
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
    }
}

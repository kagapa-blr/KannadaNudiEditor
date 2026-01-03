using System;
using System.Collections.Generic;
using System.Text;

namespace KannadaNudiEditor.Helpers.Conversion
{
    internal static partial class FileConversionUtilities
    {
        // =========================================================
        // Public conversion entry point (A2U)
        // =========================================================
        public static string ConvertAsciiToUnicode(string input, ConversionConfig cfg)
        {
            string stage0 = PreNormalizeAscii(
                input,
                cfg.AsciiHalantChar,
                cfg.AsciiJoinCharsBeforeNoSpace,
                cfg.AsciiConsonantStartChars);

            if (cfg.EnableZwnjInsertion)
                stage0 = PreInsertZwnj(stage0, cfg.AsciiHalantChar, cfg.AsciiConsonantStartChars, cfg.Zwnj);

            string mapped = ProcessStreamLongestToken(stage0, cfg);

            if (cfg.EnableKannadaClusterPostProcess)
                mapped = PostProcessKannadaClusters(mapped, cfg.DependentVowels, cfg.Halant);

            mapped = ApplyPostFixupsKannadaRuns(mapped, cfg.PostFixupsPairs);

            return mapped.Normalize(NormalizationForm.FormC);
        }

        // =========================================================
        // A2U: streaming longest-token match
        // =========================================================

        private static string ProcessStreamLongestToken(string text, ConversionConfig cfg)
        {
            if (text == null) return string.Empty;

            var sb = new StringBuilder(text.Length + Math.Min(1024, text.Length / 2));

            int i = 0;
            int n = text.Length;

            int maxToken = cfg.MaxKeyLenInMapping > 0
                ? Math.Min(cfg.MaxTokenLength, cfg.MaxKeyLenInMapping)
                : cfg.MaxTokenLength;

            while (i < n)
            {
                if (cfg.IgnoreList != null && cfg.IgnoreList.Contains(text[i]))
                {
                    i++;
                    continue;
                }

                if (TryMatchMapping(text, i, n, maxToken, cfg, out string mapped, out int matchLen))
                {
                    if (cfg.EnableZwjAfterHalant && sb.Length > 0 && sb[sb.Length - 1] == cfg.Halant)
                        sb.Append(cfg.Zwj);

                    sb.Append(mapped);
                    i += matchLen;
                    continue;
                }

                // 1-char fallback
                char ch = text[i];
                string one = ch.ToString();

                if (cfg.AsciiArkavattu.TryGetValue(one, out var ra))
                {
                    sb.Append(ra);
                    sb.Append(cfg.Halant);
                }
                else if (cfg.Vattaksharagalu.TryGetValue(one, out var baseLetter))
                {
                    sb.Append(cfg.Halant);
                    sb.Append(baseLetter);
                }
                else if (cfg.BrokenCases.TryGetValue(one, out var bc))
                {
                    ApplyBrokenCase(sb, bc, cfg);
                }
                else
                {
                    if (cfg.AsciiDigitToKannada != null && cfg.AsciiDigitToKannada.TryGetValue(ch, out var kd))
                        sb.Append(kd);
                    else if (ch == cfg.AsciiHalantChar)
                        sb.Append(cfg.Halant);
                    else
                        sb.Append(ch);
                }

                i++;
            }

            return sb.ToString();
        }
        private static bool TryMatchMapping(
            string txt,
            int pos,
            int txtLen,
            int maxToken,
            ConversionConfig cfg,
            out string mapped,
            out int matchLen)
        {
            mapped = string.Empty;
            matchLen = 0;

            var buckets = cfg.MappingKeysByLen;
            if (buckets == null) return false;

            int remaining = txtLen - pos;
            int top = Math.Min(maxToken, remaining);
            if (top <= 0) return false;

            for (int len = top; len >= 1; len--)
            {
                // Defensive: bucket array size depends on MaxTokenLength
                if ((uint)len >= (uint)buckets.Length) continue;

                var keys = buckets[len];
                if (keys.Count == 0) continue;

                for (int k = 0; k < keys.Count; k++)
                {
                    string key = keys[k];

                    // compare directly against the source string (no Substring allocation)
                    if (string.CompareOrdinal(txt, pos, key, 0, len) == 0)
                    {
                        mapped = cfg.Mapping[key];
                        matchLen = len;
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ApplyBrokenCase(StringBuilder sb, BrokenCase bc, ConversionConfig cfg)
        {
            if (sb.Length == 0) return;

            char last = sb[sb.Length - 1];
            if (cfg.DependentVowels.Contains(last) && bc.Mapping.TryGetValue(last, out char replacement))
                sb[sb.Length - 1] = replacement;
        }





        // =========================================================
        // A2U: pre-normalization / join rules
        // =========================================================
        public static string PreNormalizeAscii(
            string input,
            char asciiHalantChar,
            HashSet<char> joinCharsBeforeNoSpace,
            HashSet<char> asciiConsonantStartChars)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Remove whitespace after explicit halant marker
            input = input.Replace($"{asciiHalantChar} ", asciiHalantChar.ToString(), StringComparison.Ordinal)
                         .Replace($"{asciiHalantChar}\t", asciiHalantChar.ToString(), StringComparison.Ordinal);

            if (joinCharsBeforeNoSpace == null || joinCharsBeforeNoSpace.Count == 0)
                return input;

            asciiConsonantStartChars ??= new HashSet<char>();

            var sb = new StringBuilder(input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                // Remove whitespace only when it is clearly inside Kannada-ASCII encoding stream
                if ((ch == ' ' || ch == '\t') && i + 1 < input.Length)
                {
                    char prev = i > 0 ? input[i - 1] : '\0';
                    char next = input[i + 1];

                    bool prevLooksKannadaAscii =
                        prev >= 0x0080 || prev == asciiHalantChar || asciiConsonantStartChars.Contains(prev);

                    bool nextLooksKannadaAscii =
                        next >= 0x0080 || asciiConsonantStartChars.Contains(next) || joinCharsBeforeNoSpace.Contains(next);

                    if (prevLooksKannadaAscii && nextLooksKannadaAscii && joinCharsBeforeNoSpace.Contains(next))
                        continue;
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }

        public static string PreInsertZwnj(
            string input,
            char asciiHalantChar,
            HashSet<char> asciiConsonantStartChars,
            char zwnj)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.IndexOf(asciiHalantChar) < 0) return input;
            if (asciiConsonantStartChars == null || asciiConsonantStartChars.Count == 0) return input;

            var sb = new StringBuilder(input.Length + 16);

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                sb.Append(ch);

                if (ch == asciiHalantChar && i + 1 < input.Length)
                {
                    char next = input[i + 1];

                    // don't inject ZWNJ before plain ASCII identifiers
                    if (next <= 0x007F && (char.IsLetterOrDigit(next) || next == '_' || next == '-'))
                        continue;

                    if (asciiConsonantStartChars.Contains(next))
                        sb.Append(zwnj);
                }
            }

            return sb.ToString();
        }

        // =========================================================
        // Kannada-run post processing (unchanged from yours)
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

                // vowel + halant-chain => move chain before vowel
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

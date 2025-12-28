using System;
using System.Collections.Generic;
using System.Text;

namespace KannadaNudiEditor.Helpers
{
    internal static class FileConversionUtilities
    {
        // Kannada block U+0C80..U+0CFF + ZWJ/ZWNJ. [web:111]
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

            // Remove whitespace right after ASCII halant.
            input = input.Replace($"{asciiHalantChar} ", asciiHalantChar.ToString(), StringComparison.Ordinal)
                         .Replace($"{asciiHalantChar}\t", asciiHalantChar.ToString(), StringComparison.Ordinal);

            // Remove whitespace BEFORE configured join-chars (fixes: "vÀvÀÛ é..." -> "vÀvÀÛé...").
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

                // Ordinal replace for exact symbol sequences. [web:110]
                foreach (var (from, to) in fixups)
                {
                    if (!string.IsNullOrEmpty(from))
                        run = run.Replace(from, to ?? string.Empty, StringComparison.Ordinal);
                }

                sb.Append(run);
            }

            return sb.ToString();
        }

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

                // If vowel sign appears before a halant+consonant chain, move it after the chain.
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KannadaNudiEditor.Helpers
{
    internal static partial class FileConversionUtilities
    {
        // =========================================================
        // Public conversion entry point (U2A)
        // =========================================================
        public static string ConvertUnicodeToAscii(string input, ConversionConfig cfg)
        {
            // JS behavior: split on spaces and convert each word.
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
        // U2A core (cluster + rules + mapping)
        // =========================================================
        private static string ConvertUnicodeWordToAscii(string word, ConversionConfig cfg)
        {
            var letters = SplitKannadaLetters(word, cfg.UnicodePrevValueChars);

            var outp = new StringBuilder(word.Length * 2);
            foreach (var letter in letters)
                outp.Append(RearrangeAndReplace(letter, cfg));

            string s = RemoveZwnj(outp.ToString()); // JS removes ZWNJ.

            if (cfg.KannadaDigitToAscii.Count > 0)
                s = ReplaceByCharMap(s, cfg.KannadaDigitToAscii);

            return s;
        }

        private static string RearrangeAndReplace(string inp, ConversionConfig cfg)
        {
            if (cfg.Mapping.TryGetValue(inp, out var direct))
                return direct;

            if (cfg.UnicodeVowels.Contains(inp))
                return MapOrSelf(inp, cfg);

            inp = cfg.RegexUniVowelPlusAnusvaraVisarga.Replace(inp, m =>
            {
                string v = m.Groups["v"].Value;
                string av = m.Groups["av"].Value;
                return MapOrSelf(v, cfg) + MapOrSelf(av, cfg);
            });

            inp = cfg.RegexUniConsonantPlusVowel.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string dv = m.Groups["dv"].Value;
                return SubstituteAscii(b, dv, vattaChain: "", appendChars: null, cfg);
            });

            inp = cfg.RegexUniVattakshara.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string chain = m.Groups["chain"].Value;
                string dv = m.Groups["dv"].Value;
                return SubstituteAscii(b, dv, chain, appendChars: null, cfg);
            });

            // reph-without-ZWJ => arkavattu behavior (append asciiArkavattuChar)
            // NOTE: This must be aligned with your exact JS regex groups for full correctness.
            inp = cfg.RegexUniRephWithoutZwj.Replace(inp, m =>
            {
                string firstVattaCons = m.Groups["firstVattaCons"].Value;
                string restChain = m.Groups["restChain"].Value;
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
    }
}

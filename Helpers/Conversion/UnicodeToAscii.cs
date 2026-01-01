using System.Text;

namespace KannadaNudiEditor.Helpers.Conversion
{
    internal static partial class FileConversionUtilities
    {
        // =========================================================
        // Public conversion entry point (U2A)
        // =========================================================
        public static string ConvertUnicodeToAscii(string input, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

            // JS behavior: split on ' ' only. [file:53]
            var words = input.Split(' ', StringSplitOptions.None);
            var sb = new StringBuilder(input.Length * 2);

            for (int wi = 0; wi < words.Length; wi++)
            {
                if (wi > 0) sb.Append(' ');

                string w = words[wi];
                if (string.IsNullOrEmpty(w)) continue;

                sb.Append(ConvertUnicodeWordToAscii(w, cfg));
            }

            string outText = sb.ToString();

            // These are your extra pipeline steps (keep them). [file:53]
            outText = ApplyBrokenCasesU2A(outText, cfg);
            outText = ApplyPostFixupsPairs(outText, cfg);

            // ✅ Correct deergha handler: do NOT reorder arbitrary bytes (your old heuristic was corrupting). [file:53]
            outText = U2ADeerghaHandleSafe(outText, cfg);

            return outText;
        }

        private static string U2ADeerghaHandleSafe(string ascii, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(ascii)) return ascii;

            // Build ASCII dependent-vowel token set from Unicode dependent vowels list (excluding halant, anus/visarga, ZWJ, etc.)
            // Kannada dep vowels in Unicode: \u0CBE \u0CBF ... \u0CCC (your DefaultPrevValueChars has them) [file:53]
            var depVowelTokens = BuildAsciiDepVowelTokenSet(cfg);
            if (depVowelTokens.Count == 0) return ascii;

            bool IsVatta(char ch) => cfg.AsciiConsonantStartChars.Contains(ch); // same idea used in JS for parsing tokens [file:53]
            bool IsDepVowel(char ch) => depVowelTokens.Contains(ch);

            var arr = ascii.ToCharArray();

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == ' ') continue;

                // 4-part: dv + v1 + v2 + v3 => v1 v2 v3 dv
                if (i + 3 < arr.Length &&
                    IsDepVowel(arr[i]) &&
                    IsVatta(arr[i + 1]) &&
                    IsVatta(arr[i + 2]) &&
                    IsVatta(arr[i + 3]))
                {
                    char dv = arr[i];
                    arr[i] = arr[i + 1];
                    arr[i + 1] = arr[i + 2];
                    arr[i + 2] = arr[i + 3];
                    arr[i + 3] = dv;
                    i += 3;
                    continue;
                }

                // 3-part: dv + v1 + v2 => v1 v2 dv
                if (i + 2 < arr.Length &&
                    IsDepVowel(arr[i]) &&
                    IsVatta(arr[i + 1]) &&
                    IsVatta(arr[i + 2]))
                {
                    char dv = arr[i];
                    arr[i] = arr[i + 1];
                    arr[i + 1] = arr[i + 2];
                    arr[i + 2] = dv;
                    i += 2;
                    continue;
                }

                // 2-part: dv + v1 => v1 dv
                if (i + 1 < arr.Length &&
                    IsDepVowel(arr[i]) &&
                    IsVatta(arr[i + 1]))
                {
                    char dv = arr[i];
                    arr[i] = arr[i + 1];
                    arr[i + 1] = dv;
                    i += 1;
                    continue;
                }
            }

            return new string(arr);
        }

        private static HashSet<char> BuildAsciiDepVowelTokenSet(ConversionConfig cfg)
        {
            var set = new HashSet<char>();

            // Kannada dependent vowels (exclude halant \u0CCD, anusvara \u0C82, visarga \u0C83, ZWJ) [file:53]
            ReadOnlySpan<char> depVowels =
                "\u0CBE\u0CBF\u0CC0\u0CC1\u0CC2\u0CC3\u0CC4\u0CC6\u0CC7\u0CC8\u0CCA\u0CCB\u0CCC";

            foreach (var dv in depVowels)
            {
                // In U2A mapping, depvowel often appears as part of "base+dv" key, but your cfg.Mapping may also contain
                // direct mapping entries for dv or combinations. We only need the ASCII token(s) that represent dv. [file:53]
                if (cfg.Mapping.TryGetValue(dv.ToString(), out var mapped) && mapped.Length == 1)
                {
                    char token = mapped[0];

                    // Exclude special ASCII halant/arkavattu if they accidentally appear. [file:53]
                    if (token == cfg.AsciiHalantChar) continue;
                    if (token == cfg.AsciiArkavattuChar) continue;

                    set.Add(token);
                }
            }

            return set;
        }



        // =========================================================
        // U2A core
        // =========================================================

        private static string ConvertUnicodeWordToAscii(string word, ConversionConfig cfg)
        {
            // JS uses letters() based on unicodePrevValueChars. [file:53]
            var letters = SplitKannadaLetters(word, cfg.UnicodePrevValueChars);

            var outp = new StringBuilder(word.Length * 2);
            foreach (var letter in letters)
                outp.Append(RearrangeAndReplace(letter, cfg));

            // JS removes ZWNJ after conversion. [file:53]
            string s = RemoveZwnj(outp.ToString());

            // Kannada digits -> ASCII digits mapping. [file:53]
            if (cfg.KannadaDigitToAscii.Count > 0)
                s = ReplaceByCharMap(s, cfg.KannadaDigitToAscii);

            return s;
        }

        // =========================================================
        // Rearrange and replace (matches JS rearrangeandreplace) [file:53]
        // =========================================================
        private static string RearrangeAndReplace(string inp, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(inp))
                return inp;

            // 1) Direct mapping
            if (cfg.Mapping.TryGetValue(inp, out var direct))
                return direct;

            // 2) Independent vowels
            if (cfg.UnicodeVowels.Contains(inp))
                return MapOrSelf(inp, cfg);

            // 3) Vowel + anusvara/visarga
            inp = cfg.RegexUniVowelPlusAnusvaraVisarga.Replace(inp, m =>
            {
                string v = m.Groups["v"].Value;
                string av = m.Groups["av"].Value;
                return MapOrSelf(v, cfg) + MapOrSelf(av, cfg);
            });

            // 4) Reph without ZWJ => arkavattu behavior (JS REGEXUNIREPHWITHOUTZWJ) [file:53]
            inp = cfg.RegexUniRephWithoutZwj.Replace(inp, m =>
            {
                string baseVattaCons = m.Groups["baseVattaCons"].Value;
                string restChain = m.Groups["restChain"].Value;
                string dv = m.Groups["dv"].Value;

                // baseVattaCons may start with ZWJ; remove it for mapping key lookup
                if (baseVattaCons.Length > 0 && baseVattaCons[0] == cfg.Zwj)
                    baseVattaCons = baseVattaCons.Substring(1);

                return SubstituteAscii(
                    baseCons: baseVattaCons,
                    depVowels: dv,
                    vattaChain: restChain,
                    appendChars: cfg.AsciiArkavattuChar.ToString(),
                    cfg: cfg);
            });

            // 5) Vattakshara chain (JS REGEXUNIVATTAKSHARA) [file:53]
            inp = cfg.RegexUniVattakshara.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string chain = m.Groups["chain"].Value;
                string dv = m.Groups["dv"].Value;

                return SubstituteAscii(
                    baseCons: b,
                    depVowels: dv,
                    vattaChain: chain,
                    appendChars: null,
                    cfg: cfg);
            });

            // 6) Consonant + dependent vowel (simple) [file:53]
            inp = cfg.RegexUniConsonantPlusVowel.Replace(inp, m =>
            {
                string b = m.Groups["base"].Value;
                string dv = m.Groups["dv"].Value;

                return SubstituteAscii(
                    baseCons: b,
                    depVowels: dv,
                    vattaChain: "",
                    appendChars: null,
                    cfg: cfg);
            });

            return MapOrSelf(inp, cfg);
        }

        // =========================================================
        // SubstituteAscii (matches JS substituteascii) [file:53]
        // =========================================================
        private static string SubstituteAscii(string baseCons, string depVowels, string vattaChain, string? appendChars, ConversionConfig cfg)
        {
            var sb = new StringBuilder();

            char? dep0 = depVowels.Length > 0 ? depVowels[0] : null;
            char? dep1 = depVowels.Length > 1 ? depVowels[1] : null;

            bool dep0IsAnusOrVis = dep0.HasValue && (dep0.Value == '\u0C82' || dep0.Value == '\u0C83');

            // If dep vowel absent: map only base. [file:53]
            if (!dep0.HasValue)
            {
                sb.Append(MapOrSelf(baseCons, cfg));
            }
            else if (!dep0IsAnusOrVis)
            {
                // Join base + first dep vowel. [file:53]
                sb.Append(MapOrSelf(baseCons + dep0.Value, cfg));
            }
            else
            {
                // Only base for now; anus/visarga appended later. [file:53]
                sb.Append(MapOrSelf(baseCons, cfg));
            }

            // Add vattaksharagalu: iterate chain and append mapped consonants. [file:53]
            if (!string.IsNullOrEmpty(vattaChain))
            {
                for (int i = 0; i < vattaChain.Length; i++)
                {
                    char ch = vattaChain[i];

                    // Ignore ZWJ during match. [file:53]
                    if (ch == cfg.Zwj) continue;

                    // Expect halant + consonant pairs.
                    if (ch == cfg.Halant && i + 1 < vattaChain.Length)
                    {
                        char next = vattaChain[i + 1];
                        bool hasZwj = false;

                        if (next == cfg.Zwj && i + 2 < vattaChain.Length)
                        {
                            hasZwj = true;
                            next = vattaChain[i + 2];
                            i++; // consumed ZWJ
                        }

                        sb.Append(MapVattaksharaOrFallback(next, hasZwj, cfg));
                        i++; // consumed consonant
                    }

                }
            }

            // Append anusvara/visarga if it was first dep vowel. [file:53]
            if (dep0IsAnusOrVis)
                sb.Append(MapOrSelf(dep0.Value.ToString(), cfg));

            // Append second dep vowel if present. [file:53]
            if (dep1.HasValue)
                sb.Append(MapOrSelf(dep1.Value.ToString(), cfg));

            // Append arkavattu (ASCII) if requested. [file:53]
            if (!string.IsNullOrEmpty(appendChars))
                sb.Append(appendChars);

            return sb.ToString();
        }

        private static string MapVattaksharaOrFallback(char consonant, bool hasZwj, ConversionConfig cfg)
        {
            // 1) Best: explicit virama-based vatta mapping (your JSON has many "\u0CCD<cons>" entries)
            // Optionally support "\u0CCD\u200D<cons>" if you add such keys later.
            string keyZwj = cfg.Halant.ToString() + cfg.Zwj + consonant; // "\u0CCD\u200D<cons>"
            string keyPlain = cfg.Halant.ToString() + consonant;         // "\u0CCD<cons>"

            if (hasZwj && cfg.Mapping.TryGetValue(keyZwj, out var mZwj))
                return mZwj;

            if (cfg.Mapping.TryGetValue(keyPlain, out var mPlain))
                return mPlain;

            // 2) Fallback: legacy vattaksharagalu table keyed by consonant (e.g., "ಥ" -> "xï")
            if (cfg.Vattaksharagalu.TryGetValue(consonant.ToString(), out var vatta))
                return vatta;

            // 3) Last fallback: map consonant itself
            return MapOrSelf(consonant.ToString(), cfg);
        }


        private static string MapOrSelf(string s, ConversionConfig cfg)
            => cfg.Mapping.TryGetValue(s, out var mapped) ? mapped : s;

        public static readonly HashSet<char> DefaultPrevValueChars = new HashSet<char>
        {
            '\u0CBE','\u0CBF','\u0CC0','\u0CC1','\u0CC2','\u0CC3','\u0CC4',
            '\u0CC6','\u0CC7','\u0CC8','\u0CCA','\u0CCB','\u0CCC',
            '\u0CCD','\u0C82','\u0C83','\u200D'
        };

        private static string ReplaceByCharMap(string s, Dictionary<char, char> map)
        {
            if (string.IsNullOrEmpty(s) || map.Count == 0) return s;
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                sb.Append(map.TryGetValue(ch, out var rep) ? rep : ch);
            return sb.ToString();
        }

        // =========================================================
        // U2A: cluster splitting + ZWNJ removal (JS letters()) [file:53]
        // =========================================================
        public static List<string> SplitKannadaLetters(string txt, HashSet<char>? prevValueChars)
        {
            // Keep JS behavior (prev-value heuristic) but fix halant+consonant clustering. [file:53]
            prevValueChars ??= DefaultPrevValueChars;

            var outp = new List<string>();
            if (string.IsNullOrEmpty(txt)) return outp;

            for (int i = 0; i < txt.Length; i++)
            {
                char c = txt[i];

                bool attach =
                    prevValueChars.Contains(c) ||
                    (outp.Count > 0 &&
                     IsKannadaOrJoiner(c) &&
                     outp[^1].Length > 0 &&
                     outp[^1][^1] == '\u0CCD' &&     // previous char is halant
                     IsKannadaConsonant(c));         // current char is consonant

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
        // Missing pipeline steps (BrokenCases + PostFixups + u2adeergahandle) [file:53]
        // =========================================================

        private static string ApplyBrokenCasesU2A(string ascii, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(ascii) || cfg.BrokenCases.Count == 0)
                return ascii;

            var sb = new StringBuilder(ascii.Length);

            for (int i = 0; i < ascii.Length; i++)
            {
                char ch = ascii[i];

                if (!cfg.BrokenCases.TryGetValue(ch.ToString(), out var bc))
                {
                    sb.Append(ch);
                    continue;
                }

                // If there is a previous char, try contextual fix: prev -> mappedPrev
                if (sb.Length > 0)
                {
                    char prev = sb[sb.Length - 1];
                    if (bc.Mapping.TryGetValue(prev, out char mappedPrev))
                    {
                        sb[sb.Length - 1] = mappedPrev;
                        continue; // consume marker
                    }
                }

                // Else append default value (if present)
                if (!string.IsNullOrEmpty(bc.Value))
                    sb.Append(bc.Value);
            }

            return sb.ToString();
        }

        private static string ApplyPostFixupsPairs(string s, ConversionConfig cfg)
        {
            if (string.IsNullOrEmpty(s) || cfg.PostFixupsPairs.Count == 0)
                return s;

            foreach (var (From, To) in cfg.PostFixupsPairs)
            {
                if (!string.IsNullOrEmpty(From))
                    s = s.Replace(From, To, StringComparison.Ordinal);
            }

            return s;
        }




    }



}

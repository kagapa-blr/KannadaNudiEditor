using System.Text;
using KannadaNudiEditor.Helpers.Conversion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KannadaNudiWeb.Services
{
    public enum KeyboardLayout
    {
        Nudi,
        Baraha
    }

    public class TransliterationService
    {
        private readonly FileConversionService _conversionService;
        private readonly StringBuilder _buffer = new StringBuilder();

        public KeyboardLayout CurrentLayout { get; private set; } = KeyboardLayout.Nudi;

        // ORIGINAL MAP (Restored as the Nudi/KGP option per user request)
        private readonly Dictionary<string, string> _nudiMap = new Dictionary<string, string>
        {
            // Consonants (Defaults to Halant form)
            { "k", "ಕ್" }, { "g", "ಗ್" }, { "c", "ಚ್" }, { "j", "ಜ್" },
            { "t", "ಟ್" }, { "d", "ಡ್" }, { "N", "ಣ್" },
            { "w", "ತ್" }, { "q", "ದ್" }, { "n", "ನ್" },
            { "p", "ಪ್" }, { "b", "ಬ್" }, { "m", "ಮ್" },
            { "y", "ಯ್" }, { "r", "ರ್" }, { "l", "ಲ್" }, { "v", "ವ್" },
            { "s", "ಸ್" }, { "h", "ಹ್" }, { "L", "ಳ್" },

            // Vowels (Independent)
            { "a", "ಅ" }, { "A", "ಆ" }, { "i", "ಇ" }, { "I", "ಈ" },
            { "u", "ಉ" }, { "U", "ಊ" }, { "e", "ಎ" }, { "E", "ಏ" },
            { "o", "ಒ" }, { "O", "ಓ" },
        };

        private readonly Dictionary<string, string> _nudiVowelSigns = new Dictionary<string, string>
        {
            { "a", "" }, // 'a' removes halant (inherent vowel)
            { "A", "ಾ" },
            { "i", "ಿ" },
            { "I", "ೀ" },
            { "u", "ು" },
            { "U", "ೂ" },
            { "e", "ೆ" },
            { "E", "ೇ" },
            { "o", "ೊ" },
            { "O", "ೋ" }
        };

        // Baraha Phonetic Map
        private readonly Dictionary<string, string> _barahaMap = new Dictionary<string, string>
        {
            // Consonants (Halant default)
            { "k", "ಕ್" }, { "K", "ಖ್" }, { "g", "ಗ್" }, { "G", "ಘ್" }, { "ng", "ಂಗ್" },
            { "c", "ಚ್" }, { "ch", "ಚ್" }, { "C", "ಛ್" }, { "Ch", "ಛ್" }, { "j", "ಜ್" }, { "J", "ಝ್" }, { "nj", "ಞ್" },
            { "T", "ಟ್" }, { "Th", "ಠ್" }, { "D", "ಡ್" }, { "Dh", "ಢ್" }, { "N", "ಣ್" },
            { "t", "ತ್" }, { "th", "ಥ್" }, { "d", "ದ್" }, { "dh", "ಧ್" }, { "n", "ನ್" },
            { "p", "ಪ್" }, { "P", "ಫ್" }, { "f", "ಫ್" }, { "b", "ಬ್" }, { "B", "ಭ್" }, { "m", "ಮ್" },
            { "y", "ಯ್" }, { "r", "ರ್" }, { "l", "ಲ್" }, { "v", "ವ್" }, { "w", "ವ್" },
            { "S", "ಶ್" }, { "sh", "ಷ್" }, { "s", "ಸ್" }, { "h", "ಹ್" },
            { "L", "ಳ್" },

            // Vowels (Independent)
            { "a", "ಅ" }, { "aa", "ಆ" }, { "A", "ಆ" },
            { "i", "ಇ" }, { "ii", "ಈ" }, { "I", "ಈ" },
            { "u", "ಉ" }, { "uu", "ಊ" }, { "U", "ಊ" },
            { "R", "ಋ" }, { "Ru", "ಋ" },
            { "e", "ಎ" }, { "ee", "ಏ" }, { "E", "ಏ" },
            { "ai", "ಐ" },
            { "o", "ಒ" }, { "oo", "ಓ" }, { "O", "ಓ" },
            { "au", "ಔ" }, { "ou", "ಔ" },

            // Modifiers
            { "M", "ಂ" }, { "H", "ಃ" }
        };

        // Baraha Vowel Signs (Matras)
        private readonly Dictionary<string, string> _barahaVowelSigns = new Dictionary<string, string>
        {
            { "a", "" }, // Removes Halant
            { "aa", "ಾ" }, { "A", "ಾ" },
            { "i", "ಿ" },
            { "ii", "ೀ" }, { "I", "ೀ" },
            { "u", "ು" },
            { "uu", "ೂ" }, { "U", "ೂ" },
            { "R", "ೃ" }, { "Ru", "ೃ" },
            { "e", "ೆ" },
            { "ee", "ೇ" }, { "E", "ೇ" },
            { "ai", "ೈ" },
            { "o", "ೊ" },
            { "oo", "ೋ" }, { "O", "ೋ" },
            { "au", "ೌ" }, { "ou", "ೌ" }
        };


        public TransliterationService(FileConversionService conversionService)
        {
            _conversionService = conversionService;
        }

        public async Task InitializeAsync()
        {
            await _conversionService.InitializeAsync();
        }

        public void SetLayout(KeyboardLayout layout)
        {
            CurrentLayout = layout;
            ClearBuffer();
        }

        public (string text, int backspaceCount) GetTransliteration(string key)
        {
            if (string.IsNullOrEmpty(key)) return ("", 0);

            if (CurrentLayout == KeyboardLayout.Nudi)
            {
                return GetNudiTransliteration(key);
            }
            else
            {
                return GetBarahaTransliteration(key);
            }
        }

        private (string text, int backspaceCount) GetNudiTransliteration(string key)
        {
            // Nudi/KGP Legacy Logic:

            // If key is a vowel modifier and buffer ends in consonant key
            if (_nudiVowelSigns.ContainsKey(key) && _buffer.Length > 0 && IsNudiConsonantKey(_buffer[_buffer.Length - 1]))
            {
                string lastKey = _buffer[_buffer.Length - 1].ToString();
                string halantForm = _nudiMap[lastKey]; // e.g. "ಕ್" (2 chars: 0C95 0CCD)
                string baseForm = halantForm.TrimEnd('\u0CCD'); // e.g. "ಕ"
                string sign = _nudiVowelSigns[key];

                string replacement = baseForm + sign;
                int removeCount = halantForm.Length; // Remove the full previous sequence (e.g. 2 chars)

                _buffer.Append(key);
                return (replacement, removeCount);
            }

            // If key is a consonant
            if (_nudiMap.TryGetValue(key, out string? val))
            {
                _buffer.Append(key);
                return (val, 0);
            }

            // Default: just insert key, clear buffer
            _buffer.Clear();
            _buffer.Append(key);
            return (key, 0);
        }

        private (string text, int backspaceCount) GetBarahaTransliteration(string key)
        {
            string combinedKey = (_buffer.ToString() + key);

            if (_buffer.Length > 0)
            {
                // 1. Try to match longest sequence backwards for VOWEL MODIFIERS on CONSONANTS
                for (int i = combinedKey.Length - 1; i >= 0; i--)
                {
                    string potentialConsonantToken = combinedKey.Substring(0, i); // e.g. "k" or "kh"
                    string potentialVowelToken = combinedKey.Substring(i); // e.g. "a" or "aa"

                    if (_barahaMap.ContainsKey(potentialConsonantToken) && IsBarahaConsonant(potentialConsonantToken))
                    {
                        if (_barahaVowelSigns.ContainsKey(potentialVowelToken))
                        {
                            // Found a valid C+V combo!
                            // e.g. "kh" + "aa"

                            // To correctly calculate backspace, we need to know what was output for 'potentialConsonantToken'.
                            // The buffer contains 'potentialConsonantToken' (plus maybe previous parts of vowel if we are extending 'a' -> 'aa'?)
                            // No, the buffer only contains keys.
                            // If buffer="k", output was "ಕ್" (2 chars).
                            // If buffer="kh", output was "ಖ್" (2 chars).

                            // BUT: If buffer="ka", output was "ಕ" (1 char). Now user types 'a' -> "kaa".
                            // We need to replace "ಕ" (1 char) with "ಕಾ" (2 chars).

                            // We need to know what characters were produced by the *previous* state of the buffer.
                            // This state isn't tracked explicitly here.

                            // However, we can reconstruct what the PREVIOUS transliteration for the buffer was.
                            // But that's complex because the buffer might be "namaska". 's'+'k'+'a'.

                            // Simplification: We assume the buffer represents ONE phonetic unit being built.
                            // So if Buffer="ka", the previous output was Transliterate("ka").
                            // BackspaceCount = Length of Transliterate(Buffer).

                            // Let's verify this assumption.
                            // T1: 'k' -> Buffer="k", Out="ಕ್" (2).
                            // T2: 'a' -> Buffer="ka". Previous was "k" -> "ಕ್". Backspace=2. New="ಕ".
                            // T3: 'a' -> Buffer="kaa". Previous was "ka" -> "ಕ" (1). Backspace=1. New="ಕಾ".

                            // This logic holds if we can accurately determine the string produced by the *previous* buffer content.

                            string previousOutput = RecalculateOutput(_buffer.ToString());
                            if (string.IsNullOrEmpty(previousOutput)) previousOutput = ""; // Should generally not happen if buffer > 0

                            string consChar = _barahaMap[potentialConsonantToken];
                            string baseChar = consChar.TrimEnd('\u0CCD');
                            string matra = _barahaVowelSigns[potentialVowelToken];

                            string replacement = baseChar + matra;

                            _buffer.Append(key);
                            return (replacement, previousOutput.Length);
                        }
                    }
                }

                // 2. Check if combined key is a valid Consonant or Vowel (Extension)
                // e.g. Buffer="t", Key="h" -> "th" -> "ಥ್".
                // Previous was "t" -> "ಟ್" (2 chars).
                // We replace "ಟ್" with "ಥ್".
                if (_barahaMap.ContainsKey(combinedKey))
                {
                    string previousOutput = RecalculateOutput(_buffer.ToString());

                    _buffer.Clear();
                    _buffer.Append(combinedKey);
                    return (_barahaMap[combinedKey], previousOutput.Length);
                }
            }

            // Standard processing (Independent Char)
            if (_barahaMap.ContainsKey(key))
            {
                _buffer.Clear();
                _buffer.Append(key);
                return (_barahaMap[key], 0);
            }

            // Unmapped
            _buffer.Clear();
            _buffer.Append(key);
            return (key, 0);
        }

        // Helper to determine what string a given key buffer would have produced.
        // This effectively simulates the logic.
        private string RecalculateOutput(string bufferKeys)
        {
            if (string.IsNullOrEmpty(bufferKeys)) return "";

            // Try to match C+V
            for (int i = bufferKeys.Length - 1; i >= 0; i--)
            {
                string c = bufferKeys.Substring(0, i);
                string v = bufferKeys.Substring(i);

                if (_barahaMap.ContainsKey(c) && IsBarahaConsonant(c) && _barahaVowelSigns.ContainsKey(v))
                {
                    string cons = _barahaMap[c];
                    string baseChar = cons.TrimEnd('\u0CCD');
                    string matra = _barahaVowelSigns[v];
                    return baseChar + matra;
                }
            }

            // Try match full key
            if (_barahaMap.ContainsKey(bufferKeys))
            {
                return _barahaMap[bufferKeys];
            }

            // Fallback (likely shouldn't happen for valid sequences in buffer, but...)
            // Just return the last key's mapping if possible, or ?
            // In our logic, the buffer is cleared if not matched.
            // So if we are here, the buffer *should* be a valid sequence.
            return "";
        }

        public void RemoveLast()
        {
            if (_buffer.Length > 0)
            {
                _buffer.Length--;
            }
        }

        private bool IsNudiConsonantKey(char k)
        {
            string s = k.ToString();
            return _nudiMap.ContainsKey(s) && !_nudiVowelSigns.ContainsKey(s);
        }

        private bool IsBarahaConsonant(string k)
        {
            if (!_barahaMap.ContainsKey(k)) return false;
            string val = _barahaMap[k];
            // Check for Halant at end
            return val.EndsWith("\u0CCD");
        }

        private string GetLastTokenFromBuffer(string buffer, Dictionary<string, string> map)
        {
             for (int i = 0; i < buffer.Length; i++)
             {
                 string sub = buffer.Substring(i);
                 if (map.ContainsKey(sub)) return sub;
             }
             return "";
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
    }
}

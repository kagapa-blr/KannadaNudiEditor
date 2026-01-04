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
            // Restore logic: Buffer-based phonetic/map logic from original file

            // If key is a vowel and buffer ends in consonant key
            if (_nudiVowelSigns.ContainsKey(key) && _buffer.Length > 0 && IsNudiConsonantKey(_buffer[_buffer.Length - 1]))
            {
                string lastKey = _buffer[_buffer.Length - 1].ToString();
                string halantForm = _nudiMap[lastKey]; // e.g. "ಕ್"
                string baseForm = halantForm.TrimEnd('\u0CCD'); // e.g. "ಕ"
                string sign = _nudiVowelSigns[key];

                string replacement = baseForm + sign;

                _buffer.Append(key);
                return (replacement, 1); // Backspace 1 (the Halant Cluster), insert Full Syllable
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
            // Baraha Logic

            // 1. Check if Buffer + Key forms a multi-key consonant or vowel (e.g., 't'+'h' -> 'th' -> 'ಥ್')
            string combinedKey = (_buffer.ToString() + key);

            // Try to match longest possible suffix from buffer
            // Since we only really care about the last few chars.
            // But let's stick to the simpler buffer model: the buffer accumulates chars that *might* be modified.

            // CASE A: Modifier/Matra Application
            // If we have a consonant in buffer (last entered), and user types a vowel sign key
            if (_buffer.Length > 0)
            {
                // Find the last mapped token in the buffer.
                // This is tricky because we might have "th" which mapped to "ಥ್".
                // We need to know what the last 'unit' was.

                // Let's assume the buffer stores raw keystrokes.
                // We need to check if the *end* of the buffer corresponds to a Consonant.

                string lastToken = GetLastTokenFromBuffer(_buffer.ToString(), _barahaMap);

                if (!string.IsNullOrEmpty(lastToken) && IsBarahaConsonant(lastToken))
                {
                    // If the new key is a vowel sign start
                    if (_barahaVowelSigns.ContainsKey(key))
                    {
                         string consonant = _barahaMap[lastToken]; // "ಥ್" or "ಕ್"
                         string baseConsonant = consonant.TrimEnd('\u0CCD');
                         string matra = _barahaVowelSigns[key];

                         _buffer.Append(key);
                         return (baseConsonant + matra, 1); // Remove the consonant, add modified
                    }

                    // Check if combined key creates a specific vowel sign (e.g. 'a'+'a' -> 'aa')
                    // Wait, usually 'a' is empty matra. So 'k'+'a' -> 'ka'. Buffer is "ka".
                    // Then user types 'a'. Combined "kaa" -> "aa" matra.

                    // Actually, if we have "ka" in buffer (which output 'ಕ'), and user types 'a'.
                    // We need to replace 'ಕ' with 'ಕಾ'.
                    // The last output was 'base + matra(a)'.
                    // This gets complicated.

                    // SIMPLIFIED LOGIC:
                    // If the last operation produced a Char+Matra, and this new key extends the Matra.
                    // This requires storing state of 'Last Output Type'.

                    // Instead, let's rely on the fact that we can backspace.

                    // If buffer ends in a sequence that forms a Vowel Sign when added to:
                    // e.g. Buffer="k", Key="a". "ka" is not a key in _barahaMap.
                    // But 'a' is a vowel sign.

                    // What if Buffer="k", Key="h"? "kh" is in _barahaMap ("ಖ್").
                    if (_barahaMap.ContainsKey(combinedKey))
                    {
                         _buffer.Clear();
                         _buffer.Append(combinedKey);
                         return (_barahaMap[combinedKey], 1); // Replace 'k' ('ಕ್') with 'kh' ('ಖ್')
                    }
                }

                // Special Case: Previous char was a Vowel Sign application?
                // e.g. "ka" -> 'ಕ'. Now type 'a' -> "kaa" -> 'ಕಾ'.
                // If we treat "ka" as a unit.

                // Let's try matching combinedKey against Consonant+Vowel combinations? No, map is too big.

                // Let's look at the buffer.
                // If buffer is "ka", and key is "a".
                // Last token "ka" is NOT in _barahaMap.
                // But "k" is. And "a" is matra.
                // "aa" is also matra.

                // Attempt to split combinedKey into (Consonant Token) + (Vowel Token)
                // Iterate backwards to find longest consonant match.
                for (int i = combinedKey.Length - 1; i >= 0; i--)
                {
                    string potentialConsonant = combinedKey.Substring(0, i);
                    string potentialVowel = combinedKey.Substring(i);

                    if (_barahaMap.ContainsKey(potentialConsonant) && IsBarahaConsonant(potentialConsonant))
                    {
                        if (_barahaVowelSigns.ContainsKey(potentialVowel))
                        {
                            // Found a valid C+V combo!
                            // e.g. "kh" + "aa"

                            string consChar = _barahaMap[potentialConsonant];
                            string baseChar = consChar.TrimEnd('\u0CCD');
                            string matra = _barahaVowelSigns[potentialVowel];

                            _buffer.Append(key);
                            return (baseChar + matra, 1); // Backspace 1 (the previous state)
                        }
                    }
                }
            }

            // Standard processing (Independent Char)
            if (_barahaMap.ContainsKey(key))
            {
                _buffer.Clear();
                _buffer.Append(key);
                return (_barahaMap[key], 0);
            }

            // If we are here, it's an unmapped key.
            _buffer.Clear();
            _buffer.Append(key);
            return (key, 0);
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
            return val.EndsWith("\u0CCD");
        }

        private string GetLastTokenFromBuffer(string buffer, Dictionary<string, string> map)
        {
             // Find the longest suffix of buffer that is a key in map
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

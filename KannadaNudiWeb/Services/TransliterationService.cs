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

        // Baraha Phonetic Map (Expanded)
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

        // Nudi/KGP Layout Map (Direct Mapping)
        // Based on Nudi 4.0/5.0 Standard QWERTY map
        private readonly Dictionary<string, string> _nudiMap = new Dictionary<string, string>
        {
             // Lowercase
            { "q", "ೌ" }, { "w", "ೈ" }, { "e", "ಾ" }, { "r", "ೀ" }, { "t", "ೂ" }, { "y", "ಬ" }, { "u", "ಹ" }, { "i", "ಗ" }, { "o", "ದ" }, { "p", "ಜ" }, { "[", "ಡ" }, { "]", "̣" },
            { "a", "ೊ" }, { "s", "ೇ" }, { "d", "್" }, { "f", "ಿ" }, { "g", "ು" }, { "h", "ಪ" }, { "j", "ರ" }, { "k", "ಕ" }, { "l", "ತ" }, { ";", "ಚ" }, { "'", "ಟ" },
            { "z", "ೞ" }, { "x", "ಂ" }, { "c", "ಮ" }, { "v", "ನ" }, { "b", "ವ" }, { "n", "ಲ" }, { "m", "ಸ" }, { ",", "ಯ" }, { ".", "." }, { "/", "" },

            // Uppercase (Shift)
            { "Q", "ಔ" }, { "W", "ಐ" }, { "E", "ಆ" }, { "R", "ಈ" }, { "T", "ಊ" }, { "Y", "ಭ" }, { "U", "ಙ" }, { "I", "ಘ" }, { "O", "ಧ" }, { "P", "ಝ" }, { "{", "ಢ" }, { "}", "ಞ" },
            { "A", "ಓ" }, { "S", "ಏ" }, { "D", "ಅ" }, { "F", "ಇ" }, { "G", "ಉ" }, { "H", "ಫ" }, { "J", "ಱ" }, { "K", "ಖ" }, { "L", "ಥ" }, { ":", "ಛ" }, { "\"", "ಠ" },
            { "Z", "ಋ" }, { "X", "ಃ" }, { "C", "ಣ" }, { "V", "ಞ" }, { "B", "ಳ" }, { "N", "ಶ" }, { "M", "ಷ" }, { "<", "ಯ" } // Some duplicates or variations exist, standardized
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
            // Nudi is direct mapping, no buffer usually needed for single chars
            // However, Nudi has a specific behavior for 'f' (Virama) and 'e' (Aa) etc.
            // But they are mapped to keys.
            // The only 'state' might be if we wanted to prevent illegal combos, but standard Nudi typing just inserts char.

            // Check mapping
            if (_nudiMap.TryGetValue(key, out string? val))
            {
                // Special handling: 'd' is Virama (್).
                // If previous char was not a consonant, it might render standalone.
                // But generally we just return the char.
                return (val, 0);
            }

            // If not in map (e.g. numbers, symbols not mapped), return as is
            return (key, 0);
        }

        private (string text, int backspaceCount) GetBarahaTransliteration(string key)
        {
            // Check for vowel modifiers first if buffer has content
            if (_buffer.Length > 0)
            {
                 string combinedKey = _buffer.ToString() + key;

                 // Check if the combination matches a Vowel Sign (e.g., 'a' after 'k')
                 // But wait, the buffer holds 'k', which was transliterated to 'ಕ್'.
                 // We need to know what was TYPED.
                 // Actually, the buffer should hold the TYPED characters that haven't been 'finalized' or can be modified.

                 // Scenario 1: User typed 'k'. Output: 'ಕ್'. Buffer: 'k'.
                 // User types 'a'. Combined: 'ka'.
                 // We need to remove 'ಕ್' and output 'ಕ'.

                 // Check if the current buffer + key forms a valid Vowel Sign context
                 // The buffer usually ends in a Consonant.

                 char lastCharTyped = _buffer[_buffer.Length - 1];

                 // If last typed was consonant
                 if (IsBarahaConsonant(lastCharTyped.ToString()))
                 {
                     // Check if 'key' is a start of a vowel sign
                     // e.g. key='a'.
                     if (_barahaVowelSigns.ContainsKey(key))
                     {
                         // We are applying a vowel sign to a consonant.
                         // Get the base consonant char.
                         string consonant = _barahaMap[lastCharTyped.ToString()]; // "ಕ್"
                         string baseConsonant = consonant.TrimEnd('\u0CCD'); // "ಕ"
                         string matra = _barahaVowelSigns[key]; // "" or "ಾ"

                         _buffer.Append(key);
                         // Backspace 1 (remove half-consonant), insert full char + matra
                         return (baseConsonant + matra, 1);
                     }
                 }

                 // Scenario 2: Double letters (e.g. 'ee', 'oo') or aspirated (e.g. 'dh')
                 // User typed 'e'. Output 'ಎ'. Buffer 'e'.
                 // User types 'e'. Combined 'ee'. Output 'ಏ'.
                 // User typed 'd'. Output 'ದ್'. Buffer 'd'.
                 // User types 'h'. Combined 'dh'. Output 'ಧ್'.

                 // Check if combined key exists in map (Consonants or Vowels)
                 if (_barahaMap.ContainsKey(combinedKey))
                 {
                     _buffer.Clear();
                     _buffer.Append(combinedKey);
                     return (_barahaMap[combinedKey], 1); // Replace previous char
                 }

                 // Check if combined key exists in Vowel Signs (e.g. 'aa' after consonant)
                 // This is tricky. If buffer is 'k'+'a' (we output 'ಕ'), and user types 'a'.
                 // Typed: 'k', 'a', 'a'.
                 // Buffer: 'ka'.
                 // We need to track if we are currently building a syllable.

                 // For simplicity in this buffer model:
                 // We only keep the last "token" in buffer.
            }

            // Standard processing
            if (_barahaMap.ContainsKey(key))
            {
                _buffer.Clear(); // New independent char starts
                _buffer.Append(key);
                return (_barahaMap[key], 0);
            }

            // If key is a vowel sign but buffer is empty or invalid, it might be independent vowel?
            // No, vowels are in _barahaMap.

            // If nothing matched, clear buffer and return key
            _buffer.Clear();
            _buffer.Append(key); // Might start a new sequence? E.g. non-mapped char.
            return (key, 0);
        }

        public void RemoveLast()
        {
            if (_buffer.Length > 0)
            {
                _buffer.Length--;
            }
        }

        private bool IsBarahaConsonant(string k)
        {
            // A consonant in our map implies it has a Halant form by default.
            // And it is NOT a vowel (independent).
            // Our _barahaMap has both.
            // Vowels map to A, AA, I, II...
            // Consonants map to Ka+Halant, etc.

            if (!_barahaMap.ContainsKey(k)) return false;

            string val = _barahaMap[k];
            // Check if it ends in Halant (Virama)
            return val.EndsWith("\u0CCD");
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
    }
}

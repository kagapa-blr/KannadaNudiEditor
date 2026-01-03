using System.Text;
using KannadaNudiEditor.Helpers.Conversion;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KannadaNudiWeb.Services
{
    public class TransliterationService
    {
        private readonly FileConversionService _conversionService;
        private readonly StringBuilder _buffer = new StringBuilder();

        // Basic Nudi/Phonetic Typing Map (Simplified for this task)
        // Ideally this should be a comprehensive JSON loaded at runtime.
        // Format: Key Sequence -> Output Unicode
        private readonly Dictionary<string, string> _typingMap = new Dictionary<string, string>
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

            // Dependent Vowel Signs (Matras) - Logic handled in code:
            // If previous char was consonant (Halant), replace Halant with Vowel Sign.
        };

        private readonly Dictionary<string, string> _vowelSigns = new Dictionary<string, string>
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

        public TransliterationService(FileConversionService conversionService)
        {
            _conversionService = conversionService;
        }

        public async Task InitializeAsync()
        {
            await _conversionService.InitializeAsync();
        }

        // Returns (TextToInsert, BackspaceCount)
        // BackspaceCount indicates how many characters to remove from the editor *before* inserting TextToInsert.
        public (string text, int backspaceCount) GetTransliteration(string key)
        {
            if (string.IsNullOrEmpty(key)) return ("", 0);

            // 1. Check if we are starting a new sequence or continuing
            // Ideally, we track state.
            // Let's assume _buffer holds the *unconverted* keys of the current syllable.

            // If key is a vowel and buffer ends in consonant key:
            if (_vowelSigns.ContainsKey(key) && _buffer.Length > 0 && IsConsonantKey(_buffer[_buffer.Length - 1]))
            {
                // We are modifying the previous consonant.
                // Previous output was likely the Halant form (e.g., 'k' -> 'ಕ್').
                // We need to backspace 2 characters (Consonant + Halant \u0CCD) and insert Consonant + Vowel Sign.
                // Or simply: Backspace 1 (the Halant) and insert Vowel Sign?
                // Wait. 'ಕ್' is 2 chars: 'ಕ' + '್'.
                // If input is 'k', we inserted 'ಕ್'.
                // Now input 'a'. We want 'ಕ'.
                // 'a' maps to "" (remove halant).
                // So we backspace 1 (remove '್') and insert ""? Result 'ಕ'. Correct.

                // If input 'A' (aa). We want 'ಕಾ'.
                // Backspace 1 (remove '್') insert 'ಾ'. Result 'ಕಾ'. Correct.

                _buffer.Append(key);
                string sign = _vowelSigns[key];

                // Logic: Backspace 1 (the halant).
                return (sign, 1);
            }

            // If key is a consonant
            if (_typingMap.TryGetValue(key, out string? val))
            {
                _buffer.Append(key);
                return (val, 0);
            }

            // Default: just insert key, clear buffer
            _buffer.Clear();
            _buffer.Append(key);
            return (key, 0);
        }

        private bool IsConsonantKey(char k)
        {
            // Simple check if key was in our consonant map keys
            string s = k.ToString();
            return _typingMap.ContainsKey(s) && !_vowelSigns.ContainsKey(s);
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
    }
}

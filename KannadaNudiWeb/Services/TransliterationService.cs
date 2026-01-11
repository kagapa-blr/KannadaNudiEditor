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

        public (string text, int backspaceCount) GetTransliteration(string key)
        {
            if (string.IsNullOrEmpty(key)) return ("", 0);

            // If key is a vowel and buffer ends in consonant key
            if (_vowelSigns.ContainsKey(key) && _buffer.Length > 0 && IsConsonantKey(_buffer[_buffer.Length - 1]))
            {
                // We need to replace the Halant form with the Vowel form.
                // Previous output: 'ಕ್' (Consonant + Halant)
                // New output: 'ಕ' (Consonant + Inherent Vowel) or 'ಕಾ' (Consonant + Vowel Sign)

                string lastKey = _buffer[_buffer.Length - 1].ToString();
                string halantForm = _typingMap[lastKey]; // e.g. "ಕ್"
                string baseForm = halantForm.TrimEnd('\u0CCD'); // e.g. "ಕ"
                string sign = _vowelSigns[key];

                string replacement = baseForm + sign;

                _buffer.Append(key);
                return (replacement, 1); // Backspace 1 (the Halant Cluster), insert Full Syllable
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

        public void RemoveLast()
        {
            if (_buffer.Length > 0)
            {
                _buffer.Length--;
            }
        }

        private bool IsConsonantKey(char k)
        {
            string s = k.ToString();
            return _typingMap.ContainsKey(s) && !_vowelSigns.ContainsKey(s);
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
    }
}

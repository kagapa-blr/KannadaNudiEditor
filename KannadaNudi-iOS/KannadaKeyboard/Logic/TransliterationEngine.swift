import Foundation

enum KeyboardLayout {
    case nudi
    case baraha
}

struct TransliterationResult {
    let text: String
    let backspaceCount: Int
}

class TransliterationEngine {
    private var buffer = ""
    var currentLayout: KeyboardLayout = .baraha

    // MARK: - Maps

    // Nudi Map (Direct/Legacy)
    private let nudiMap: [String: String] = [
        // Consonants
        "k": "ಕ್", "g": "ಗ್", "c": "ಚ್", "j": "ಜ್",
        "t": "ಟ್", "d": "ಡ್", "N": "ಣ್",
        "w": "ತ್", "q": "ದ್", "n": "ನ್",
        "p": "ಪ್", "b": "ಬ್", "m": "ಮ್",
        "y": "ಯ್", "r": "ರ್", "l": "ಲ್", "v": "ವ್",
        "s": "ಸ್", "h": "ಹ್", "L": "ಳ್",

        // Vowels
        "a": "ಅ", "A": "ಆ", "i": "ಇ", "I": "ಈ",
        "u": "ಉ", "U": "ಊ", "e": "ಎ", "E": "ಏ",
        "o": "ಒ", "O": "ಓ"
    ]

    private let nudiVowelSigns: [String: String] = [
        "a": "", // 'a' removes halant
        "A": "ಾ",
        "i": "ಿ",
        "I": "ೀ",
        "u": "ು",
        "U": "ೂ",
        "e": "ೆ",
        "E": "ೇ",
        "o": "ೊ",
        "O": "ೋ"
    ]

    // Baraha Map (Phonetic)
    private let barahaMap: [String: String] = [
        // Consonants (Halant default)
        "k": "ಕ್", "K": "ಖ್", "g": "ಗ್", "G": "ಘ್", "ng": "ಂಗ್",
        "c": "ಚ್", "ch": "ಚ್", "C": "ಛ್", "Ch": "ಛ್", "j": "ಜ್", "J": "ಝ್", "nj": "ಞ್",
        "T": "ಟ್", "Th": "ಠ್", "D": "ಡ್", "Dh": "ಢ್", "N": "ಣ್",
        "t": "ತ್", "th": "ಥ್", "d": "ದ್", "dh": "ಧ್", "n": "ನ್",
        "p": "ಪ್", "P": "ಫ್", "f": "ಫ್", "b": "ಬ್", "B": "ಭ್", "m": "ಮ್",
        "y": "ಯ್", "r": "ರ್", "l": "ಲ್", "v": "ವ್", "w": "ವ್",
        "S": "ಶ್", "sh": "ಷ್", "s": "ಸ್", "h": "ಹ್",
        "L": "ಳ್",

        // Vowels (Independent)
        "a": "ಅ", "aa": "ಆ", "A": "ಆ",
        "i": "ಇ", "ii": "ಈ", "I": "ಈ",
        "u": "ಉ", "uu": "ಊ", "U": "ಊ",
        "R": "ಋ", "Ru": "ಋ",
        "e": "ಎ", "ee": "ಏ", "E": "ಏ",
        "ai": "ಐ",
        "o": "ಒ", "oo": "ಓ", "O": "ಓ",
        "au": "ಔ", "ou": "ಔ",

        // Modifiers
        "M": "ಂ", "H": "ಃ"
    ]

    private let barahaVowelSigns: [String: String] = [
        "a": "",
        "aa": "ಾ", "A": "ಾ",
        "i": "ಿ",
        "ii": "ೀ", "I": "ೀ",
        "u": "ು",
        "uu": "ೂ", "U": "ೂ",
        "R": "ೃ", "Ru": "ೃ",
        "e": "ೆ",
        "ee": "ೇ", "E": "ೇ",
        "ai": "ೈ",
        "o": "ೊ",
        "oo": "ೋ", "O": "ೋ",
        "au": "ೌ", "ou": "ೌ"
    ]

    // MARK: - Methods

    func setLayout(_ layout: KeyboardLayout) {
        currentLayout = layout
        clearBuffer()
    }

    func clearBuffer() {
        buffer = ""
    }

    func removeLast() {
        if !buffer.isEmpty {
            buffer.removeLast()
        }
    }

    func getTransliteration(key: String) -> TransliterationResult {
        if key.isEmpty {
            return TransliterationResult(text: "", backspaceCount: 0)
        }

        if currentLayout == .nudi {
            return getNudiTransliteration(key: key)
        } else {
            return getBarahaTransliteration(key: key)
        }
    }

    private func getNudiTransliteration(key: String) -> TransliterationResult {
        // Nudi/KGP Legacy Logic:

        // If key is a vowel modifier and buffer ends in consonant key
        if nudiVowelSigns.keys.contains(key), !buffer.isEmpty {
            let lastKey = String(buffer.last!)
            if isNudiConsonantKey(lastKey) {
                if let halantForm = nudiMap[lastKey], let sign = nudiVowelSigns[key] {
                    // halantForm e.g. "ಕ್" (0C95 0CCD)
                    let baseForm = halantForm.trimmingCharacters(in: CharacterSet(charactersIn: "\u{0CCD}"))
                    let replacement = baseForm + sign

                    // Swift string count might count grapheme clusters, but input proxy usually deletes by character code units?
                    // Actually, iOS backspace count is usually by character.
                    // "ಕ್" is 2 chars.
                    let removeCount = halantForm.count

                    buffer.append(key)
                    return TransliterationResult(text: replacement, backspaceCount: removeCount)
                }
            }
        }

        // If key is a consonant
        if let val = nudiMap[key] {
            buffer.append(key)
            return TransliterationResult(text: val, backspaceCount: 0)
        }

        // Default
        buffer = ""
        buffer.append(key)
        return TransliterationResult(text: key, backspaceCount: 0)
    }

    private func getBarahaTransliteration(key: String) -> TransliterationResult {
        let combinedKey = buffer + key

        if !buffer.isEmpty {
            // 1. Try to match longest sequence backwards for VOWEL MODIFIERS on CONSONANTS
            let combinedCount = combinedKey.count
            // Swift strings are tricky with indices. Using integer loops for easier porting logic.
            // But we need to handle unicode properly if keys are unicode. Assuming keys are ASCII/single chars for now as per maps.

            for i in (0..<combinedCount).reversed() {
                // i is the split point.
                // substring(0, i) -> prefix
                // substring(i) -> suffix

                let index = combinedKey.index(combinedKey.startIndex, offsetBy: i)
                let potentialConsonantToken = String(combinedKey[..<index])
                let potentialVowelToken = String(combinedKey[index...])

                if let _ = barahaMap[potentialConsonantToken], isBarahaConsonant(potentialConsonantToken) {
                    if let matra = barahaVowelSigns[potentialVowelToken] {
                        // Found C+V combo
                        let previousOutput = recalculateOutput(bufferKeys: buffer)

                        let consChar = barahaMap[potentialConsonantToken]!
                        let baseChar = consChar.trimmingCharacters(in: CharacterSet(charactersIn: "\u{0CCD}"))

                        let replacement = baseChar + matra

                        buffer.append(key)
                        return TransliterationResult(text: replacement, backspaceCount: previousOutput.count)
                    }
                }
            }

            // 2. Check if combined key is a valid Consonant or Vowel (Extension)
            if let val = barahaMap[combinedKey] {
                let previousOutput = recalculateOutput(bufferKeys: buffer)
                buffer = ""
                buffer.append(combinedKey)
                return TransliterationResult(text: val, backspaceCount: previousOutput.count)
            }
        }

        // Standard processing
        if let val = barahaMap[key] {
            buffer = ""
            buffer.append(key)
            return TransliterationResult(text: val, backspaceCount: 0)
        }

        // Unmapped
        buffer = ""
        buffer.append(key)
        return TransliterationResult(text: key, backspaceCount: 0)
    }

    private func recalculateOutput(bufferKeys: String) -> String {
        if bufferKeys.isEmpty { return "" }

        let count = bufferKeys.count
        for i in (0..<count).reversed() {
            let index = bufferKeys.index(bufferKeys.startIndex, offsetBy: i)
            let c = String(bufferKeys[..<index])
            let v = String(bufferKeys[index...])

            if let _ = barahaMap[c], isBarahaConsonant(c), let matra = barahaVowelSigns[v] {
                let cons = barahaMap[c]!
                let baseChar = cons.trimmingCharacters(in: CharacterSet(charactersIn: "\u{0CCD}"))
                return baseChar + matra
            }
        }

        if let val = barahaMap[bufferKeys] {
            return val
        }

        return ""
    }

    private func isNudiConsonantKey(_ k: String) -> Bool {
        return nudiMap.keys.contains(k) && !nudiVowelSigns.keys.contains(k)
    }

    private func isBarahaConsonant(_ k: String) -> Bool {
        guard let value = barahaMap[k] else { return false }
        return value.hasSuffix("\u{0CCD}")
    }
}

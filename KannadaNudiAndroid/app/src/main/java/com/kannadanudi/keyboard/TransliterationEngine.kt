package com.kannadanudi.keyboard

import java.lang.StringBuilder

enum class KeyboardLayout {
    Nudi,
    Baraha
}

data class TransliterationResult(val text: String, val backspaceCount: Int)

class TransliterationEngine {
    private val buffer = StringBuilder()
    var currentLayout = KeyboardLayout.Baraha

    // Maps porting from C# TransliterationService.cs

    // Nudi Map (Direct/Legacy)
    private val nudiMap = mapOf(
        // Consonants
        "k" to "ಕ್", "g" to "ಗ್", "c" to "ಚ್", "j" to "ಜ್",
        "t" to "ಟ್", "d" to "ಡ್", "N" to "ಣ್",
        "w" to "ತ್", "q" to "ದ್", "n" to "ನ್",
        "p" to "ಪ್", "b" to "ಬ್", "m" to "ಮ್",
        "y" to "ಯ್", "r" to "ರ್", "l" to "ಲ್", "v" to "ವ್",
        "s" to "ಸ್", "h" to "ಹ್", "L" to "ಳ್",

        // Vowels
        "a" to "ಅ", "A" to "ಆ", "i" to "ಇ", "I" to "ಈ",
        "u" to "ಉ", "U" to "ಊ", "e" to "ಎ", "E" to "ಏ",
        "o" to "ಒ", "O" to "ಓ"
    )

    private val nudiVowelSigns = mapOf(
        "a" to "", // 'a' removes halant
        "A" to "ಾ",
        "i" to "ಿ",
        "I" to "ೀ",
        "u" to "ು",
        "U" to "ೂ",
        "e" to "ೆ",
        "E" to "ೇ",
        "o" to "ೊ",
        "O" to "ೋ"
    )

    // Baraha Map (Phonetic)
    private val barahaMap = mapOf(
        // Consonants (Halant default)
        "k" to "ಕ್", "K" to "ಖ್", "g" to "ಗ್", "G" to "ಘ್", "ng" to "ಂಗ್",
        "c" to "ಚ್", "ch" to "ಚ್", "C" to "ಛ್", "Ch" to "ಛ್", "j" to "ಜ್", "J" to "ಝ್", "nj" to "ಞ್",
        "T" to "ಟ್", "Th" to "ಠ್", "D" to "ಡ್", "Dh" to "ಢ್", "N" to "ಣ್",
        "t" to "ತ್", "th" to "ಥ್", "d" to "ದ್", "dh" to "ಧ್", "n" to "ನ್",
        "p" to "ಪ್", "P" to "ಫ್", "f" to "ಫ್", "b" to "ಬ್", "B" to "ಭ್", "m" to "ಮ್",
        "y" to "ಯ್", "r" to "ರ್", "l" to "ಲ್", "v" to "ವ್", "w" to "ವ್",
        "S" to "ಶ್", "sh" to "ಷ್", "s" to "ಸ್", "h" to "ಹ್",
        "L" to "ಳ್",

        // Vowels (Independent)
        "a" to "ಅ", "aa" to "ಆ", "A" to "ಆ",
        "i" to "ಇ", "ii" to "ಈ", "I" to "ಈ",
        "u" to "ಉ", "uu" to "ಊ", "U" to "ಊ",
        "R" to "ಋ", "Ru" to "ಋ",
        "e" to "ಎ", "ee" to "ಏ", "E" to "ಏ",
        "ai" to "ಐ",
        "o" to "ಒ", "oo" to "ಓ", "O" to "ಓ",
        "au" to "ಔ", "ou" to "ಔ",

        // Modifiers
        "M" to "ಂ", "H" to "ಃ"
    )

    private val barahaVowelSigns = mapOf(
        "a" to "",
        "aa" to "ಾ", "A" to "ಾ",
        "i" to "ಿ",
        "ii" to "ೀ", "I" to "ೀ",
        "u" to "ು",
        "uu" to "ೂ", "U" to "ೂ",
        "R" to "ೃ", "Ru" to "ೃ",
        "e" to "ೆ",
        "ee" to "ೇ", "E" to "ೇ",
        "ai" to "ೈ",
        "o" to "ೊ",
        "oo" to "ೋ", "O" to "ೋ",
        "au" to "ೌ", "ou" to "ೌ"
    )

    fun setLayout(layout: KeyboardLayout) {
        currentLayout = layout
        clearBuffer()
    }

    fun clearBuffer() {
        buffer.setLength(0)
    }

    fun removeLast() {
        if (buffer.isNotEmpty()) {
            buffer.setLength(buffer.length - 1)
        }
    }

    fun getTransliteration(key: String): TransliterationResult {
        if (key.isEmpty()) return TransliterationResult("", 0)

        return if (currentLayout == KeyboardLayout.Nudi) {
            getNudiTransliteration(key)
        } else {
            getBarahaTransliteration(key)
        }
    }

    private fun getNudiTransliteration(key: String): TransliterationResult {
        // Nudi/KGP Legacy Logic:

        // If key is a vowel modifier and buffer ends in consonant key
        if (nudiVowelSigns.containsKey(key) && buffer.isNotEmpty() && isNudiConsonantKey(buffer.last())) {
            val lastKey = buffer.last().toString()
            val halantForm = nudiMap[lastKey]!! // e.g. "ಕ್" (2 chars: 0C95 0CCD)
            val baseForm = halantForm.trimEnd('\u0CCD') // e.g. "ಕ"
            val sign = nudiVowelSigns[key]!!

            val replacement = baseForm + sign
            val removeCount = halantForm.length // Remove the full previous sequence

            buffer.append(key)
            return TransliterationResult(replacement, removeCount)
        }

        // If key is a consonant
        if (nudiMap.containsKey(key)) {
            buffer.append(key)
            return TransliterationResult(nudiMap[key]!!, 0)
        }

        // Default: just insert key, clear buffer
        buffer.setLength(0)
        buffer.append(key)
        return TransliterationResult(key, 0)
    }

    private fun getBarahaTransliteration(key: String): TransliterationResult {
        val combinedKey = buffer.toString() + key

        if (buffer.isNotEmpty()) {
            // 1. Try to match longest sequence backwards for VOWEL MODIFIERS on CONSONANTS
            for (i in combinedKey.length - 1 downTo 0) {
                val potentialConsonantToken = combinedKey.substring(0, i)
                val potentialVowelToken = combinedKey.substring(i)

                if (barahaMap.containsKey(potentialConsonantToken) && isBarahaConsonant(potentialConsonantToken)) {
                    if (barahaVowelSigns.containsKey(potentialVowelToken)) {
                        // Found C+V combo
                        val previousOutput = recalculateOutput(buffer.toString())

                        val consChar = barahaMap[potentialConsonantToken]!!
                        val baseChar = consChar.trimEnd('\u0CCD')
                        val matra = barahaVowelSigns[potentialVowelToken]!!

                        val replacement = baseChar + matra

                        buffer.append(key)
                        return TransliterationResult(replacement, previousOutput.length)
                    }
                }
            }

            // 2. Check if combined key is a valid Consonant or Vowel (Extension)
            if (barahaMap.containsKey(combinedKey)) {
                val previousOutput = recalculateOutput(buffer.toString())
                buffer.setLength(0)
                buffer.append(combinedKey)
                return TransliterationResult(barahaMap[combinedKey]!!, previousOutput.length)
            }
        }

        // Standard processing
        if (barahaMap.containsKey(key)) {
            buffer.setLength(0)
            buffer.append(key)
            return TransliterationResult(barahaMap[key]!!, 0)
        }

        // Unmapped
        buffer.setLength(0)
        buffer.append(key)
        return TransliterationResult(key, 0)
    }

    private fun recalculateOutput(bufferKeys: String): String {
        if (bufferKeys.isEmpty()) return ""

        // Try to match C+V
        for (i in bufferKeys.length - 1 downTo 0) {
            val c = bufferKeys.substring(0, i)
            val v = bufferKeys.substring(i)

            if (barahaMap.containsKey(c) && isBarahaConsonant(c) && barahaVowelSigns.containsKey(v)) {
                val cons = barahaMap[c]!!
                val baseChar = cons.trimEnd('\u0CCD')
                val matra = barahaVowelSigns[v]!!
                return baseChar + matra
            }
        }

        // Try match full key
        if (barahaMap.containsKey(bufferKeys)) {
            return barahaMap[bufferKeys]!!
        }

        return ""
    }

    private fun isNudiConsonantKey(k: Char): Boolean {
        val s = k.toString()
        return nudiMap.containsKey(s) && !nudiVowelSigns.containsKey(s)
    }

    private fun isBarahaConsonant(k: String): Boolean {
        val value = barahaMap[k] ?: return false
        return value.endsWith("\u0CCD")
    }
}

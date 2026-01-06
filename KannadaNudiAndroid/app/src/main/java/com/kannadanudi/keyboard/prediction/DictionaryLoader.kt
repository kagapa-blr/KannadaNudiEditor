package com.kannadanudi.keyboard.prediction

import android.content.Context
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.io.BufferedReader
import java.io.InputStreamReader

class DictionaryLoader(private val context: Context) {
    private val trie = Trie()
    private var isLoaded = false

    fun loadDictionary() {
        GlobalScope.launch(Dispatchers.IO) {
            try {
                val inputStream = context.assets.open("kannada_wordList.txt")
                val reader = BufferedReader(InputStreamReader(inputStream))

                var line: String? = reader.readLine()
                while (line != null) {
                    val word = line.trim()
                    if (word.isNotEmpty()) {
                        trie.insert(word)
                    }
                    line = reader.readLine()
                }
                reader.close()
                isLoaded = true
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    fun getSuggestions(prefix: String): List<String> {
        if (!isLoaded || prefix.isEmpty()) return emptyList()
        return trie.searchPrefix(prefix)
    }
}

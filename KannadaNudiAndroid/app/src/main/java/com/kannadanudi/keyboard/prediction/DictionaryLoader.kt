package com.kannadanudi.keyboard.prediction

import android.content.Context
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch
import java.io.BufferedReader
import java.io.InputStreamReader

class DictionaryLoader(private val context: Context) {
    private val trie = Trie()
    @Volatile
    private var isLoaded = false
    private val scope = CoroutineScope(Dispatchers.IO + Job())

    fun loadDictionary() {
        scope.launch {
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

    fun cancel() {
        // scope.cancel() // Job cancellation if needed
    }
}

package com.kannadanudi.keyboard.prediction

class Trie {
    private val root = TrieNode()

    fun insert(word: String) {
        var node = root
        for (char in word) {
            node = node.children.computeIfAbsent(char) { TrieNode() }
        }
        node.isEndOfWord = true
    }

    fun searchPrefix(prefix: String, limit: Int = 10): List<String> {
        val results = ArrayList<String>()
        var node = root

        // Navigate to the end of the prefix
        for (char in prefix) {
            node = node.children[char] ?: return emptyList()
        }

        // Collect words from this node
        collectWords(node, prefix, results, limit)
        return results
    }

    private fun collectWords(node: TrieNode, currentPrefix: String, results: MutableList<String>, limit: Int) {
        if (results.size >= limit) return

        if (node.isEndOfWord) {
            results.add(currentPrefix)
        }

        // To keep it simple and somewhat deterministic, we iterate through keys.
        // For better quality, we might want to prioritize based on frequency if available,
        // but the wordlist is flat.
        // Sort keys to ensure alphabetical order since we switched to HashMap
        val sortedKeys = node.children.keys.sorted()
        for (char in sortedKeys) {
            val childNode = node.children[char]!!
            if (results.size >= limit) return
            collectWords(childNode, currentPrefix + char, results, limit)
        }
    }
}

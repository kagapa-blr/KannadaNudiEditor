package com.kannadanudi.keyboard.prediction

class TrieNode {
    val children = HashMap<Char, TrieNode>()
    var isEndOfWord = false
}

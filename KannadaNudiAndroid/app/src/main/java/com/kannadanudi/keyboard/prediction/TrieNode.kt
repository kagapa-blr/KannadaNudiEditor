package com.kannadanudi.keyboard.prediction

import java.util.TreeMap

class TrieNode {
    val children = TreeMap<Char, TrieNode>()
    var isEndOfWord = false
}

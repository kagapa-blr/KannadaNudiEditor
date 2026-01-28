import Foundation

class TrieNode {
    var children: [Character: TrieNode] = [:]
    var isEndOfWord: Bool = false
}

class Trie {
    private let root = TrieNode()

    func insert(_ word: String) {
        var node = root
        for char in word {
            if node.children[char] == nil {
                node.children[char] = TrieNode()
            }
            node = node.children[char]!
        }
        node.isEndOfWord = true
    }

    func searchPrefix(_ prefix: String, limit: Int = 10) -> [String] {
        var results = [String]()
        var node = root

        // Navigate to the end of the prefix
        for char in prefix {
            guard let child = node.children[char] else {
                return []
            }
            node = child
        }

        collectWords(node: node, currentPrefix: prefix, results: &results, limit: limit)
        return results
    }

    private func collectWords(node: TrieNode, currentPrefix: String, results: inout [String], limit: Int) {
        if results.count >= limit { return }

        if node.isEndOfWord {
            results.append(currentPrefix)
        }

        // Sort keys to ensure deterministic order (alphabetical)
        let sortedKeys = node.children.keys.sorted()

        for char in sortedKeys {
            if results.count >= limit { return }
            if let childNode = node.children[char] {
                collectWords(node: childNode, currentPrefix: currentPrefix + String(char), results: &results, limit: limit)
            }
        }
    }
}

import Foundation

class DictionaryLoader {
    private let trie = Trie()
    private var isLoaded = false
    private let queue = DispatchQueue(label: "com.kannadanudi.dictionary", qos: .userInitiated)

    func loadDictionary() {
        queue.async {
            self.load()
        }
    }

    private func load() {
        guard let path = Bundle.main.path(forResource: "kannada_wordList", ofType: "txt") else {
            print("Dictionary file not found")
            return
        }

        do {
            let content = try String(contentsOfFile: path, encoding: .utf8)
            let lines = content.components(separatedBy: .newlines)

            for line in lines {
                let word = line.trimmingCharacters(in: .whitespacesAndNewlines)
                if !word.isEmpty {
                    trie.insert(word)
                }
            }
            isLoaded = true
        } catch {
            print("Error loading dictionary: \(error)")
        }
    }

    func getSuggestions(_ prefix: String) -> [String] {
        return queue.sync {
            if !isLoaded || prefix.isEmpty { return [] }
            return trie.searchPrefix(prefix)
        }
    }
}

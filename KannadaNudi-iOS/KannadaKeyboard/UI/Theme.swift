import SwiftUI

struct Theme {
    // Karnataka Flag Colors
    static let karnatakaRed = Color(red: 1.0, green: 0.0, blue: 0.0) // #FF0000
    static let karnatakaYellow = Color(red: 1.0, green: 1.0, blue: 0.0) // #FFFF00

    // UI Component Colors
    static let keyboardBackground = karnatakaYellow

    static let keyNormalBackground = Color.white
    static let keyNormalText = Color.black

    static let keySpecialBackground = karnatakaRed
    static let keySpecialText = Color.white

    static let keyShadow = Color.black.opacity(0.3)
}

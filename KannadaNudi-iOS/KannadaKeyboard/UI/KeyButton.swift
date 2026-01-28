import SwiftUI

struct KeyButton: View {
    let label: String
    let width: CGFloat?
    let height: CGFloat
    let action: () -> Void
    let isPressed: Bool

    init(label: String, width: CGFloat? = nil, height: CGFloat = 50, isPressed: Bool = false, action: @escaping () -> Void) {
        self.label = label
        self.width = width
        self.height = height
        self.isPressed = isPressed
        self.action = action
    }

    var body: some View {
        Button(action: action) {
            Text(label)
                .font(.system(size: 20, weight: .medium))
                .foregroundColor(.black)
                .frame(maxWidth: width == nil ? .infinity : width, maxHeight: height)
                .background(
                    RoundedRectangle(cornerRadius: 6)
                        .fill(Theme.keyBackground)
                        .shadow(color: Theme.keyShadow, radius: 1, x: 0, y: 1)
                )
                .overlay(
                    RoundedRectangle(cornerRadius: 6)
                        .stroke(Color.gray.opacity(0.2), lineWidth: 1)
                )
                .scaleEffect(isPressed ? 0.95 : 1.0)
        }
    }
}

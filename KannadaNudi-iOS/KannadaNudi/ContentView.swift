import SwiftUI

struct ContentView: View {
    var body: some View {
        VStack(spacing: 20) {
            Image(systemName: "keyboard")
                .font(.system(size: 60))
                .foregroundColor(.blue)

            Text("Kannada Nudi Keyboard")
                .font(.title)
                .bold()

            Text("To enable the keyboard:")
                .font(.headline)

            VStack(alignment: .leading, spacing: 10) {
                StepView(number: 1, text: "Open Settings > General > Keyboard")
                StepView(number: 2, text: "Tap 'Keyboards' > 'Add New Keyboard...'")
                StepView(number: 3, text: "Select 'KannadaNudi' under Third-Party Keyboards")
                StepView(number: 4, text: "Tap 'KannadaNudi - KannadaKeyboard'")
                StepView(number: 5, text: "Toggle 'Allow Full Access' (Optional, for better performance)")
            }
            .padding()
            .background(Color(UIColor.secondarySystemBackground))
            .cornerRadius(10)

            Spacer()
        }
        .padding()
    }
}

struct StepView: View {
    let number: Int
    let text: String

    var body: some View {
        HStack(alignment: .top) {
            Text("\(number).")
                .bold()
            Text(text)
        }
    }
}

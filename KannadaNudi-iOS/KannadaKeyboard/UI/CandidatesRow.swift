import SwiftUI

struct CandidatesRow: View {
    let candidates: [String]
    let onSelect: (String) -> Void

    var body: some View {
        ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 10) {
                ForEach(candidates, id: \.self) { candidate in
                    Button(action: { onSelect(candidate) }) {
                        Text(candidate)
                            .font(.system(size: 18))
                            .foregroundColor(.black)
                            .padding(.vertical, 8)
                            .padding(.horizontal, 16)
                            .background(Theme.karnatakaYellow)
                            .cornerRadius(8)
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(Theme.karnatakaRed, lineWidth: 1)
                            )
                    }
                }
            }
            .padding(.horizontal, 4)
            .padding(.vertical, 4)
        }
        .background(Color(UIColor.systemGray6))
        .frame(height: 50)
    }
}

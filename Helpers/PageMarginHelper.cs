namespace KannadaNudiEditor.Helpers
{
    public static class PageMarginHelper
    {
        public static Dictionary<string, List<double>> pageMarginsCollection = new Dictionary<string, List<double>>();


        public static List<PageMargins> GetPresetMargins(bool isEnglish = true)
        {
            return new List<PageMargins>
            {
                new PageMargins { Key = "Normal", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1 in", right = "Right: 1 in" },
                new PageMargins { Key = "Narrow", top = "Top: 0.5 in", bottom = "Bottom: 0.5 in", left = "Left: 0.5 in", right = "Right: 0.5 in" },
                new PageMargins { Key = "Moderate", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 0.75 in", right = "Right: 0.75 in" },
                new PageMargins { Key = "Wide", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 2 in", right = "Right: 2 in" },
                new PageMargins { Key = "Mirrored", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1.25 in", right = "Right: 1 in" },
                new PageMargins { Key = "Office 2003", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1.25 in", right = "Right: 1.25 in" },
                new PageMargins { Key = "Compact", top = "Top: 0.25 in", bottom = "Bottom: 0.25 in", left = "Left: 0.25 in", right = "Right: 0.25 in" },
                new PageMargins { Key = "Expanded", top = "Top: 1.5 in", bottom = "Bottom: 1.5 in", left = "Left: 1.5 in", right = "Right: 1.5 in" },
                new PageMargins { Key = "Thesis", top = "Top: 1.5 in", bottom = "Bottom: 1.5 in", left = "Left: 1.5 in", right = "Right: 1 in" },
                new PageMargins { Key = "Legal", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1.25 in", right = "Right: 1 in" },
                new PageMargins { Key = "Book Manuscript", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1.5 in", right = "Right: 1 in" },
                new PageMargins { Key = "Resume", top = "Top: 0.75 in", bottom = "Bottom: 0.75 in", left = "Left: 0.75 in", right = "Right: 0.75 in" },
                new PageMargins { Key = "A4 Default", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 0.75 in", right = "Right: 0.75 in" },
                new PageMargins { Key = "IEEE Format", top = "Top: 0.75 in", bottom = "Bottom: 1 in", left = "Left: 0.625 in", right = "Right: 0.625 in" },
                new PageMargins { Key = "APA Format", top = "Top: 1 in", bottom = "Bottom: 1 in", left = "Left: 1 in", right = "Right: 1 in" },
                new PageMargins { Key = "Envelope", top = "Top: 0.5 in", bottom = "Bottom: 0.5 in", left = "Left: 1 in", right = "Right: 1 in" },
                new PageMargins { Key = "Presentation Print", top = "Top: 1.25 in", bottom = "Bottom: 1.25 in", left = "Left: 1.25 in", right = "Right: 1.25 in" },
                new PageMargins {
                    Key = "Custom",
                    top = isEnglish ? "Set custom margins" : "ಗ್ರಾಹಕೀಯ ಅಂಚುಗಳು",
                    bottom = "",
                    left = "",
                    right = ""
                }
            };
        }

        public static Dictionary<string, List<double>> GetMarginValues()
        {
            return new Dictionary<string, List<double>>
            {
                { "Normal", new List<double> { 1, 1, 1, 1 } },
                { "Narrow", new List<double> { 0.5, 0.5, 0.5, 0.5 } },
                { "Moderate", new List<double> { 1, 1, 0.75, 0.75 } },
                { "Wide", new List<double> { 1, 1, 2, 2 } },
                { "Mirrored", new List<double> { 1, 1, 1.25, 1 } },
                { "Office 2003", new List<double> { 1, 1, 1.25, 1.25 } },
                { "Compact", new List<double> { 0.25, 0.25, 0.25, 0.25 } },
                { "Expanded", new List<double> { 1.5, 1.5, 1.5, 1.5 } },
                { "Thesis", new List<double> { 1.5, 1.5, 1.5, 1 } },
                { "Legal", new List<double> { 1, 1, 1.25, 1 } },
                { "Book Manuscript", new List<double> { 1, 1, 1.5, 1 } },
                { "Resume", new List<double> { 0.75, 0.75, 0.75, 0.75 } },
                { "A4 Default", new List<double> { 1, 1, 0.75, 0.75 } },
                { "IEEE Format", new List<double> { 0.75, 1, 0.625, 0.625 } },
                { "APA Format", new List<double> { 1, 1, 1, 1 } },
                { "Envelope", new List<double> { 0.5, 0.5, 1, 1 } },
                { "Presentation Print", new List<double> { 1.25, 1.25, 1.25, 1.25 } }
            };
        }
    }

    public class PageMargins
    {
        public string? Key { get; set; }
        public string? top { get; set; }
        public string? bottom { get; set; }
        public string? left { get; set; }
        public string? right { get; set; }

        public override string ToString()
        {
            return $"{Key}: {top}, {bottom}, {left}, {right}";
        }
    }
}

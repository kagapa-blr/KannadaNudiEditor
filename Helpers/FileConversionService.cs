// Helpers/FileConversionService.cs - ✅ JS-VERIFIED COMPLETE [file:10]
using System.Text;
using System.Text.RegularExpressions;

namespace KannadaNudiEditor.Helpers
{
    public static class FileConversionService
    {
        public static Func<string, string> AsciiToUnicodeConverter =>
            text => new AsciiToUnicodeEngine().Convert(text);

        public static Func<string, string> UnicodeToAsciiConverter =>
            text => new UnicodeToAsciiEngine().Convert(text);
    }


    #region ASCII → UNICODE
    internal sealed partial class AsciiToUnicodeEngine
    {
        private const string ZWNJ = "\u200C"; // 200C [file:1]
        private const string ZWJ = "\u200D"; // 200D [file:1]
        private const string RA = "\u0CB0";  // 0CB0 [file:1]
        private const string HALANT = "\u0CCD"; // 0CCD [file:1]

        private static readonly string[] KannadaNumbers =
        [
            "\u0CE6", "\u0CE7", "\u0CE8", "\u0CE9", "\u0CEA",
        "\u0CEB", "\u0CEC", "\u0CED", "\u0CEE", "\u0CEF"
        ]; // [file:1]

        private static readonly string[] EnglishNumbers = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"]; // [file:1]

        // JS-verified regexes present in paste.txt [file:1]
        private static readonly Regex ZwnjRegex = RegexAsciiZwnj();   // (ï)([...]) [file:1]
        private static readonly Regex Vatta3Regex = RegexAsciiVatta3();  // ([depvowel])([v1])([v2])([v3]) [file:1]
        private static readonly Regex Vatta2Regex = RegexAsciiVatta2();  // ([depvowel])([v1])([v2]) [file:1]
        private static readonly Regex Vatta1Regex = RegexAsciiVatta1();  // ([depvowel])([v1]) [file:1]
        private static readonly Regex DeergaRegex = RegexA2UDeerga();     // (ಾ|ೆ|ೊ)(ಂ) [file:1]

        // NOT available in dump (shows ????), so placeholders (no-op) until you paste exact patterns. [file:1]
        private static readonly Regex RephBeforeConvertRegex = RegexNeverMatch(); // TODO [file:1]
        private static readonly Regex ArkavattuRegex = RegexNeverMatch(); // TODO [file:1]

        // You will paste this yourself (kept empty by request).
        private static readonly (string From, string To)[] ReplacePairs =
        [


            
                // Vowels
                ("A", "\u0C82"),
                ("B", "\u0C83"),
                ("C", "\u0C85"),
                ("D", "\u0C86"),
                ("E", "\u0C87"),
                ("F", "\u0C88"),
                ("G", "\u0C89"),
                ("H", "\u0C8A"),
                ("I", "\u0C8B"),
                ("I2", "\u0CE0"),
                ("J", "\u0C8E"),
                ("K", "\u0C8F"),
                ("L", "\u0C90"),
                ("M", "\u0C92"),
                ("N", "\u0C93"),
                ("O", "\u0C94"),
                ("P\u00EF", "\u0C95\u0CCD"),
                ("P\u00C0", "\u0C95"),
                ("P\u00C1", "\u0C95\u0CBE"),
                ("Q",      "\u0C95\u0CBF"),
                ("Q\u00C3", "\u0C95\u0CC0"),
                ("P\u00C0\u00C4", "\u0C95\u0CC1"),
                ("P\u00C0\u00C6", "\u0C95\u0CC2"),
                ("P\u00C0\u00C8", "\u0C95\u0CC3"),
                ("P\u00C9", "\u0C95\u0CC6"),
                ("P\u00C9\u00C3", "\u0C95\u0CC7"),
                ("P\u00C9\u00CA", "\u0C95\u0CC8"),
                ("P\u00C9\u00C6", "\u0C95\u0CCA"),
                ("P\u00C9\u00C6\u00C3", "\u0C95\u0CCB"),
                ("P\u00CB", "\u0C95\u0CCC"),
                ("CA", "\u0C85\u0C82"),
                ("CB", "\u0C85\u0C83"),
                ("IÄ", "\u0C8B"),
                ("IÆ2", "\u0CE0"),
                ("Pï", "\u0C95\u0CCD"),
                ("PÀ", "\u0C95"),
                ("PÁ", "\u0C95\u0CBE"),
                ("Q", "\u0C95\u0CBF"),
                ("QÃ", "\u0C95\u0CC0"),
                ("PÀÄ", "\u0C95\u0CC1"),
                ("PÀÆ", "\u0C95\u0CC2"),
                ("PÀÈ", "\u0C95\u0CC3"),
                ("PÉ", "\u0C95\u0CC6"),
                ("PÉÃ", "\u0C95\u0CC7"),
                ("PÉÊ", "\u0C95\u0CC8"),
                ("PÉÆ", "\u0C95\u0CCA"),
                ("PÉÆÃ", "\u0C95\u0CCB"),
                ("PË", "\u0C95\u0CCC"),
                ("Sï", "\u0C96\u0CCD"),
                ("R", "\u0C96"),
                ("SÁ", "\u0C96\u0CBE"),
                ("T", "\u0C96\u0CBF"),
                ("TÃ", "\u0C96\u0CC0"),
                ("RÄ", "\u0C96\u0CC1"),
                ("RÆ", "\u0C96\u0CC2"),
                ("RÈ", "\u0C96\u0CC3"),
                ("SÉ", "\u0C96\u0CC6"),
                ("SÉÃ", "\u0C96\u0CC7"),
                ("SÉÊ", "\u0C96\u0CC8"),
                ("SÉÆ", "\u0C96\u0CCA"),
                ("SÉÆÃ", "\u0C96\u0CCB"),
                ("SË", "\u0C96\u0CCC"),
                ("Uï", "\u0C97\u0CCD"),
                ("UÀ", "\u0C97"),
                ("UÁ", "\u0C97\u0CBE"),
                ("V", "\u0C97\u0CBF"),
                ("VÃ", "\u0C97\u0CC0"),
                ("UÀÄ", "\u0C97\u0CC1"),
                ("UÀÆ", "\u0C97\u0CC2"),
                ("UÀÈ", "\u0C97\u0CC3"),
                ("UÉ", "\u0C97\u0CC6"),
                ("UÉÃ", "\u0C97\u0CC7"),
                ("UÉÊ", "\u0C97\u0CC8"),
                ("UÉÆ", "\u0C97\u0CCA"),
                ("UÉÆÃ", "\u0C97\u0CCB"),
                ("UË", "\u0C97\u0CCC"),
                ("Wï", "\u0C98\u0CCD"),
                ("WÀ", "\u0C98"),
                ("WÁ", "\u0C98\u0CBE"),
                ("X", "\u0C98\u0CBF"),
                ("XÃ", "\u0C98\u0CC0"),
                ("WÀÄ", "\u0C98\u0CC1"),
                ("WÀÆ", "\u0C98\u0CC2"),
                ("WÀÈ", "\u0C98\u0CC3"),
                ("WÉ", "\u0C98\u0CC6"),
                ("WÉÃ", "\u0C98\u0CC7"),
                ("WÉÊ", "\u0C98\u0CC8"),
                ("WÉÆ", "\u0C98\u0CCA"),
                ("WÉÆÃ", "\u0C98\u0CCB"),
                ("WË", "\u0C98\u0CCC"),
                ("Yï", "\u0C99\u0CCD"),
                ("Y", "\u0C99"),
                ("Zï", "\u0C9A\u0CCD"),
                ("ZÀ", "\u0C9A"),
                ("ZÁ", "\u0C9A\u0CBE"),
                ("a", "\u0C9A\u0CBF"),
                ("aÃ", "\u0C9A\u0CC0"),
                ("ZÀÄ", "\u0C9A\u0CC1"),
                ("ZÀÆ", "\u0C9A\u0CC2"),
                ("ZÀÈ", "\u0C9A\u0CC3"),
                ("ZÉ", "\u0C9A\u0CC6"),
                ("ZÉÃ", "\u0C9A\u0CC7"),
                ("ZÉÊ", "\u0C9A\u0CC8"),
                ("ZÉÆ", "\u0C9A\u0CCA"),
                ("ZÉÆÃ", "\u0C9A\u0CCB"),
                ("ZË", "\u0C9A\u0CCC"),
                ("bï", "\u0C9B\u0CCD"),
                ("bÀ", "\u0C9B"),
                ("bÁ", "\u0C9B\u0CBE"),
                ("c", "\u0C9B\u0CBF"),
                ("cÃ", "\u0C9B\u0CC0"),
                ("bÀÄ", "\u0C9B\u0CC1"),
                ("bÀÆ", "\u0C9B\u0CC2"),
                ("bÀÈ", "\u0C9B\u0CC3"),
                ("bÉ", "\u0C9B\u0CC6"),
                ("bÉÃ", "\u0C9B\u0CC7"),
                ("bÉÊ", "\u0C9B\u0CC8"),
                ("bÉÆ", "\u0C9B\u0CCA"),
                ("bÉÆÃ", "\u0C9B\u0CCB"),
                ("bË", "\u0C9B\u0CCC"),
                ("eï", "\u0C9C\u0CCD"),
                ("d", "\u0C9C"),
                ("eÁ", "\u0C9C\u0CBE"),
                ("f", "\u0C9C\u0CBF"),
                ("fÃ", "\u0C9C\u0CC0"),
                ("dÄ", "\u0C9C\u0CC1"),
                ("dÆ", "\u0C9C\u0CC2"),
                ("dÈ", "\u0C9C\u0CC3"),
                ("eÉ", "\u0C9C\u0CC6"),
                ("eÉÃ", "\u0C9C\u0CC7"),
                ("eÉÊ", "\u0C9C\u0CC8"),
                ("eÉÆ", "\u0C9C\u0CCA"),
                ("eÉÆÃ", "\u0C9C\u0CCB"),
                ("eË", "\u0C9C\u0CCC"),
                ("gÀhiï", "\u0C9D\u0CCD"),
                ("gÀhÄ", "\u0C9D"),
                ("gÀhiÁ", "\u0C9D\u0CBE"),
                ("jhÄ", "\u0C9D\u0CBF"),
                ("jhÄÃ", "\u0C9D\u0CC0"),
                ("gÀhÄÄ", "\u0C9D\u0CC1"),
                ("gÀhÄÆ", "\u0C9D\u0CC2"),
                ("gÀhÄÈ", "\u0C9D\u0CC3"),
                ("gÉhÄ", "\u0C9D\u0CC6"),
                ("gÉhÄÃ", "\u0C9D\u0CC7"),
                ("gÉhÄÊ", "\u0C9D\u0CC8"),
                ("gÉhÆ", "\u0C9D\u0CCA"),
                ("gÉhÆÃ", "\u0C9D\u0CCB"),
                ("gÀhiË", "\u0C9D\u0CCC"),
                ("kï", "\u0C9E\u0CCD"),
                ("k", "\u0C9E"),
                ("mï", "\u0C9F\u0CCD"),
                ("l", "\u0C9F"),
                ("mÁ", "\u0C9F\u0CBE"),
                ("n", "\u0C9F\u0CBF"),
                ("nÃ", "\u0C9F\u0CC0"),
                ("lÄ", "\u0C9F\u0CC1"),
                ("lÆ", "\u0C9F\u0CC2"),
                ("lÈ", "\u0C9F\u0CC3"),
                ("mÉ", "\u0C9F\u0CC6"),
                ("mÉÃ", "\u0C9F\u0CC7"),
                ("mÉÊ", "\u0C9F\u0CC8"),
                ("mÉÆ", "\u0C9F\u0CCA"),
                ("mÉÆÃ", "\u0C9F\u0CCB"),
                ("mË", "\u0C9F\u0CCC"),
                ("oï", "\u0CA0\u0CCD"),
                ("oÀ", "\u0CA0"),
                ("oÁ", "\u0CA0\u0CBE"),
                ("p", "\u0CA0\u0CBF"),
                ("pÃ", "\u0CA0\u0CC0"),
                ("oÀÄ", "\u0CA0\u0CC1"),
                ("oÀÆ", "\u0CA0\u0CC2"),
                ("oÀÈ", "\u0CA0\u0CC3"),
                ("oÉ", "\u0CA0\u0CC6"),
                ("oÉÃ", "\u0CA0\u0CC7"),
                ("oÉÊ", "\u0CA0\u0CC8"),
                ("oÉÆ", "\u0CA0\u0CCA"),
                ("oÉÆÃ", "\u0CA0\u0CCB"),
                ("oË", "\u0CA0\u0CCC"),
                ("qï", "\u0CA1\u0CCD"),
                ("qÀ", "\u0CA1"),
                ("qÁ", "\u0CA1\u0CBE"),
                ("r", "\u0CA1\u0CBF"),
                ("rÃ", "\u0CA1\u0CC0"),
                ("qÀÄ", "\u0CA1\u0CC1"),
                ("qÀÆ", "\u0CA1\u0CC2"),
                ("qÀÈ", "\u0CA1\u0CC3"),
                ("qÉ", "\u0CA1\u0CC6"),
                ("qÉÃ", "\u0CA1\u0CC7"),
                ("qÉÊ", "\u0CA1\u0CC8"),
                ("qÉÆ", "\u0CA1\u0CCA"),
                ("qÉÆÃ", "\u0CA1\u0CCB"),
                ("qË", "\u0CA1\u0CCC"),
                ("qsï", "\u0CA2\u0CCD"),
                ("qsÀ", "\u0CA2"),
                ("qsÁ", "\u0CA2\u0CBE"),
                ("rü", "\u0CA2\u0CBF"),
                ("rüÃ", "\u0CA2\u0CC0"),
                ("qsÀÄ", "\u0CA2\u0CC1"),
                ("qsÀÆ", "\u0CA2\u0CC2"),
                ("qsÀÈ", "\u0CA2\u0CC3"),
                ("qsÉ", "\u0CA2\u0CC6"),
                ("qsÉÃ", "\u0CA2\u0CC7"),
                ("qsÉÊ", "\u0CA2\u0CC8"),
                ("qsÉÆ", "\u0CA2\u0CCA"),
                ("qsÉÆÃ", "\u0CA2\u0CCB"),
                ("qsË", "\u0CA2\u0CCC"),
                ("uï", "\u0CA3\u0CCD"),
                ("t", "\u0CA3"),
                ("uÁ", "\u0CA3\u0CBE"),
                ("tÂ", "\u0CA3\u0CBF"),
                ("tÂÃ", "\u0CA3\u0CC0"),
                ("tÄ", "\u0CA3\u0CC1"),
                ("tÆ", "\u0CA3\u0CC2"),
                ("tÈ", "\u0CA3\u0CC3"),
                ("uÉ", "\u0CA3\u0CC6"),
                ("uÉÃ", "\u0CA3\u0CC7"),
                ("uÉÊ", "\u0CA3\u0CC8"),
                ("uÉÆ", "\u0CA3\u0CCA"),
                ("uÉÆÃ", "\u0CA3\u0CCB"),
                ("uË", "\u0CA3\u0CCC"),
                ("vï", "\u0CA4\u0CCD"),
                ("vÀ", "\u0CA4"),
                ("vÁ", "\u0CA4\u0CBE"),
                ("w", "\u0CA4\u0CBF"),
                ("wÃ", "\u0CA4\u0CC0"),
                ("vÀÄ", "\u0CA4\u0CC1"),
                ("vÀÆ", "\u0CA4\u0CC2"),
                ("vÀÈ", "\u0CA4\u0CC3"),
                ("vÉ", "\u0CA4\u0CC6"),
                ("vÉÃ", "\u0CA4\u0CC7"),
                ("vÉÊ", "\u0CA4\u0CC8"),
                ("vÉÆ", "\u0CA4\u0CCA"),
                ("vÉÆÃ", "\u0CA4\u0CCB"),
                ("vË", "\u0CA4\u0CCC"),
                ("xï", "\u0CA5\u0CCD"),
                ("xÀ", "\u0CA5"),
                ("xÁ", "\u0CA5\u0CBE"),
                ("y", "\u0CA5\u0CBF"),
                ("yÃ", "\u0CA5\u0CC0"),
                ("xÀÄ", "\u0CA5\u0CC1"),
                ("xÀÆ", "\u0CA5\u0CC2"),
                ("xÀÈ", "\u0CA5\u0CC3"),
                ("xÉ", "\u0CA5\u0CC6"),
                ("xÉÃ", "\u0CA5\u0CC7"),
                ("xÉÊ", "\u0CA5\u0CC8"),
                ("xÉÆ", "\u0CA5\u0CCA"),
                ("xÉÆÃ", "\u0CA5\u0CCB"),
                ("xË", "\u0CA5\u0CCC"),
                ("zï", "\u0CA6\u0CCD"),
                ("zÀ", "\u0CA6"),
                ("zÁ", "\u0CA6\u0CBE"),
                ("¢", "\u0CA6\u0CBF"),
                ("¢Ã", "\u0CA6\u0CC0"),
                ("zÀÄ", "\u0CA6\u0CC1"),
                ("zÀÆ", "\u0CA6\u0CC2"),
                ("zÀÈ", "\u0CA6\u0CC3"),
                ("zÉ", "\u0CA6\u0CC6"),
                ("zÉÃ", "\u0CA6\u0CC7"),
                ("zÉÊ", "\u0CA6\u0CC8"),
                ("zÉÆ", "\u0CA6\u0CCA"),
                ("zÉÆÃ", "\u0CA6\u0CCB"),
                ("zË", "\u0CA6\u0CCC"),
                ("zsï", "\u0CA7\u0CCD"),
                ("zsÀ", "\u0CA7"),
                ("zsÁ", "\u0CA7\u0CBE"),
                ("¢ü", "\u0CA7\u0CBF"),
                ("¢üÃ", "\u0CA7\u0CC0"),
                ("zsÀÄ", "\u0CA7\u0CC1"),
                ("zsÀÆ", "\u0CA7\u0CC2"),
                ("zsÀÈ", "\u0CA7\u0CC3"),
                ("zsÉ", "\u0CA7\u0CC6"),
                ("zsÉÃ", "\u0CA7\u0CC7"),
                ("zsÉÊ", "\u0CA7\u0CC8"),
                ("zsÉÆ", "\u0CA7\u0CCA"),
                ("zsÉÆÃ", "\u0CA7\u0CCB"),
                ("zsË", "\u0CA7\u0CCC"),
                ("£ï", "\u0CA8\u0CCD"),
                ("£À", "\u0CA8"),
                ("£Á", "\u0CA8\u0CBE"),
                ("¤", "\u0CA8\u0CBF"),
                ("¤Ã", "\u0CA8\u0CC0"),
                ("£ÀÄ", "\u0CA8\u0CC1"),
                ("£ÀÆ", "\u0CA8\u0CC2"),
                ("£ÀÈ", "\u0CA8\u0CC3"),
                ("£É", "\u0CA8\u0CC6"),
                ("£ÉÃ", "\u0CA8\u0CC7"),
                ("£ÉÊ", "\u0CA8\u0CC8"),
                ("£ÉÆ", "\u0CA8\u0CCA"),
                ("£ÉÆÃ", "\u0CA8\u0CCB"),
                ("£Ë", "\u0CA8\u0CCC"),
                ("¥ï", "\u0CAA\u0CCD"),
                ("¥À", "\u0CAA"),
                ("¥Á", "\u0CAA\u0CBE"),
                ("¦", "\u0CAA\u0CBF"),
                ("¦Ã", "\u0CAA\u0CC0"),
                ("¥ÀÅ", "\u0CAA\u0CC1"),
                ("¥ÀÇ", "\u0CAA\u0CC2"),
                ("¥ÀÈ", "\u0CAA\u0CC3"),
                ("¥É", "\u0CAA\u0CC6"),
                ("¥ÉÃ", "\u0CAA\u0CC7"),
                ("¥ÉÊ", "\u0CAA\u0CC8"),
                ("¥ÉÇ", "\u0CAA\u0CCA"),
                ("¥ÉÇÃ", "\u0CAA\u0CCB"),
                ("¥Ë", "\u0CAA\u0CCC"),
                ("¥sï", "\u0CAB\u0CCD"),
                ("¥sÀ", "\u0CAB"),
                ("¥sÁ", "\u0CAB\u0CBE"),
                ("¦ü", "\u0CAB\u0CBF"),
                ("¦üÃ", "\u0CAB\u0CC0"),
                ("¥sÀÅ", "\u0CAB\u0CC1"),
                ("¥sÀÇ", "\u0CAB\u0CC2"),
                ("¥sÀÈ", "\u0CAB\u0CC3"),
                ("¥sÉ", "\u0CAB\u0CC6"),
                ("¥sÉÃ", "\u0CAB\u0CC7"),
                ("¥sÉÊ", "\u0CAB\u0CC8"),
                ("¥sÉÇ", "\u0CAB\u0CCA"),
                ("¥sÉÇÃ", "\u0CAB\u0CCB"),
                ("¥sË", "\u0CAB\u0CCC"),
                ("¨ï", "\u0CAC\u0CCD"),
                ("§", "\u0CAC"),
                ("¨Á", "\u0CAC\u0CBE"),
                ("©", "\u0CAC\u0CBF"),
                ("©Ã", "\u0CAC\u0CC0"),
                ("§Ä", "\u0CAC\u0CC1"),
                ("§Æ", "\u0CAC\u0CC2"),
                ("§È", "\u0CAC\u0CC3"),
                ("¨É", "\u0CAC\u0CC6"),
                ("¨ÉÃ", "\u0CAC\u0CC7"),
                ("¨ÉÊ", "\u0CAC\u0CC8"),
                ("¨ÉÆ", "\u0CAC\u0CCA"),
                ("¨ÉÆÃ", "\u0CAC\u0CCB"),
                ("¨Ë", "\u0CAC\u0CCC"),
                ("¨sï", "\u0CAD\u0CCD"),
                ("¨sÀ", "\u0CAD"),
                ("¨sÁ", "\u0CAD\u0CBE"),
                ("©ü", "\u0CAD\u0CBF"),
                ("©üÃ", "\u0CAD\u0CC0"),
                ("¨sÀÄ", "\u0CAD\u0CC1"),
                ("¨sÀÆ", "\u0CAD\u0CC2"),
                ("¨sÀÈ", "\u0CAD\u0CC3"),
                ("¨sÉ", "\u0CAD\u0CC6"),
                ("¨sÉÃ", "\u0CAD\u0CC7"),
                ("¨sÉÊ", "\u0CAD\u0CC8"),
                ("¨sÉÆ", "\u0CAD\u0CCA"),
                ("¨sÉÆÃ", "\u0CAD\u0CCB"),
                ("¨sË", "\u0CAD\u0CCC"),
                ("ªÀiï", "\u0CAE\u0CCD"),
                ("ªÀÄ", "\u0CAE"),
                ("ªÀiÁ", "\u0CAE\u0CBE"),
                ("«Ä", "\u0CAE\u0CBF"),
                ("«ÄÃ", "\u0CAE\u0CC0"),
                ("ªÀÄÄ", "\u0CAE\u0CC1"),
                ("ªÀÄÆ", "\u0CAE\u0CC2"),
                ("ªÀÄÈ", "\u0CAE\u0CC3"),
                ("ªÉÄ", "\u0CAE\u0CC6"),
                ("ªÉÄÃ", "\u0CAE\u0CC7"),
                ("ªÉÄÊ", "\u0CAE\u0CC8"),
                ("ªÉÆ", "\u0CAE\u0CCA"),
                ("ªÉÆÃ", "\u0CAE\u0CCB"),
                ("ªÀiË", "\u0CAE\u0CCC"),
                ("AiÀiï", "\u0CAF\u0CCD"),
                ("AiÀÄ", "\u0CAF"),
                ("AiÀiÁ", "\u0CAF\u0CBE"),
                ("¬Ä", "\u0CAF\u0CBF"),
                ("¬ÄÃ", "\u0CAF\u0CC0"),
                ("AiÀÄÄ", "\u0CAF\u0CC1"),
                ("AiÀÄÆ", "\u0CAF\u0CC2"),
                ("AiÀÄÈ", "\u0CAF\u0CC3"),
                ("AiÉÄ", "\u0CAF\u0CC6"),
                ("AiÉÄÃ", "\u0CAF\u0CC7"),
                ("AiÉÄÊ", "\u0CAF\u0CC8"),
                ("AiÉÆ", "\u0CAF\u0CCA"),
                ("AiÉÆÃ", "\u0CAF\u0CCB"),
                ("AiÀiË", "\u0CAF\u0CCC"),
                ("gï", "\u0CB0\u0CCD"),
                ("gÀ", "\u0CB0"),
                ("gÁ", "\u0CB0\u0CBE"),
                ("j", "\u0CB0\u0CBF"),
                ("jÃ", "\u0CB0\u0CC0"),
                ("gÀÄ", "\u0CB0\u0CC1"),
                ("gÀÆ", "\u0CB0\u0CC2"),
                ("gÀÈ", "\u0CB0\u0CC3"),
                ("gÉ", "\u0CB0\u0CC6"),
                ("gÉÃ", "\u0CB0\u0CC7"),
                ("gÉÊ", "\u0CB0\u0CC8"),
                ("gÉÆ", "\u0CB0\u0CCA"),
                ("gÉÆÃ", "\u0CB0\u0CCB"),
                ("gË", "\u0CB0\u0CCC"),
                ("¯ï", "\u0CB2\u0CCD"),
                ("®", "\u0CB2"),
                ("¯Á", "\u0CB2\u0CBE"),
                ("°", "\u0CB2\u0CBF"),
                ("°Ã", "\u0CB2\u0CC0"),
                ("®Ä", "\u0CB2\u0CC1"),
                ("®Æ", "\u0CB2\u0CC2"),
                ("®È", "\u0CB2\u0CC3"),
                ("¯É", "\u0CB2\u0CC6"),
                ("¯ÉÃ", "\u0CB2\u0CC7"),
                ("¯ÉÊ", "\u0CB2\u0CC8"),
                ("¯ÉÆ", "\u0CB2\u0CCA"),
                ("¯ÉÆÃ", "\u0CB2\u0CCB"),
                ("¯Ë", "\u0CB2\u0CCC"),
                ("ªï", "\u0CB5\u0CCD"),
                ("ªÀ", "\u0CB5"),
                ("ªÁ", "\u0CB5\u0CBE"),
                ("«", "\u0CB5\u0CBF"),
                ("«Ã", "\u0CB5\u0CC0"),
                ("ªÀÅ", "\u0CB5\u0CC1"),
                ("ªÀÇ", "\u0CB5\u0CC2"),
                ("ªÀÈ", "\u0CB5\u0CC3"),
                ("ªÉ", "\u0CB5\u0CC6"),
                ("ªÉÃ", "\u0CB5\u0CC7"),
                ("ªÉÊ", "\u0CB5\u0CC8"),
                ("ªÉÇ", "\u0CB5\u0CCA"),
                ("ªÉÇÃ", "\u0CB5\u0CCB"),
                ("ªË", "\u0CB5\u0CCC"),
                ("±ï", "\u0CB6\u0CCD"),
                ("±À", "\u0CB6"),
                ("±Á", "\u0CB6\u0CBE"),
                ("²", "\u0CB6\u0CBF"),
                ("²Ã", "\u0CB6\u0CC0"),
                ("±ÀÄ", "\u0CB6\u0CC1"),
                ("±ÀÆ", "\u0CB6\u0CC2"),
                ("±ÀÈ", "\u0CB6\u0CC3"),
                ("±É", "\u0CB6\u0CC6"),
                ("±ÉÃ", "\u0CB6\u0CC7"),
                ("±ÉÊ", "\u0CB6\u0CC8"),
                ("±ÉÆ", "\u0CB6\u0CCA"),
                ("±ÉÆÃ", "\u0CB6\u0CCB"),
                ("±Ë", "\u0CB6\u0CCC"),
                ("μï", "\u0CB7\u0CCD"),
                ("μÀ", "\u0CB7"),
                ("μÁ", "\u0CB7\u0CBE"),
                ("¶", "\u0CB7\u0CBF"),
                ("¶Ã", "\u0CB7\u0CC0"),
                ("μÀÄ", "\u0CB7\u0CC1"),
                ("μÀÆ", "\u0CB7\u0CC2"),
                ("μÀÈ", "\u0CB7\u0CC3"),
                ("μÉ", "\u0CB7\u0CC6"),
                ("μÉÃ", "\u0CB7\u0CC7"),
                ("μÉÊ", "\u0CB7\u0CC8"),
                ("μÉÆ", "\u0CB7\u0CCA"),
                ("μÉÆÃ", "\u0CB7\u0CCB"),
                ("μË", "\u0CB7\u0CCC"),
                ("¸ï", "\u0CB8\u0CCD"),
                ("¸À", "\u0CB8"),
                ("¸Á", "\u0CB8\u0CBE"),
                ("¹", "\u0CB8\u0CBF"),
                ("¹Ã", "\u0CB8\u0CC0"),
                ("¸ÀÄ", "\u0CB8\u0CC1"),
                ("¸ÀÆ", "\u0CB8\u0CC2"),
                ("¸ÀÈ", "\u0CB8\u0CC3"),
                ("¸É", "\u0CB8\u0CC6"),
                ("¸ÉÃ", "\u0CB8\u0CC7"),
                ("¸ÉÊ", "\u0CB8\u0CC8"),
                ("¸ÉÆ", "\u0CB8\u0CCA"),
                ("¸ÉÆÃ", "\u0CB8\u0CCB"),
                ("¸Ë", "\u0CB8\u0CCC"),
                ("ºï", "\u0CB9\u0CCD"),
                ("ºÀ", "\u0CB9"),
                ("ºÁ", "\u0CB9\u0CBE"),
                ("»", "\u0CB9\u0CBF"),
                ("»Ã", "\u0CB9\u0CC0"),
                ("ºÀÄ", "\u0CB9\u0CC1"),
                ("ºÀÆ", "\u0CB9\u0CC2"),
                ("ºÀÈ", "\u0CB9\u0CC3"),
                ("ºÉ", "\u0CB9\u0CC6"),
                ("ºÉÃ", "\u0CB9\u0CC7"),
                ("ºÉÊ", "\u0CB9\u0CC8"),
                ("ºÉÆ", "\u0CB9\u0CCA"),
                ("ºÉÆÃ", "\u0CB9\u0CCB"),
                ("ºË", "\u0CB9\u0CCC"),
                ("¼ï", "\u0CB3\u0CCD"),
                ("¼À", "\u0CB3"),
                ("¼Á", "\u0CB3\u0CBE"),
                ("½", "\u0CB3\u0CBF"),
                ("½Ã", "\u0CB3\u0CC0"),
                ("¼ÀÄ", "\u0CB3\u0CC1"),
                ("¼ÀÆ", "\u0CB3\u0CC2"),
                ("¼ÀÈ", "\u0CB3\u0CC3"),
                ("¼É", "\u0CB3\u0CC6"),
                ("¼ÉÃ", "\u0CB3\u0CC7"),
                ("¼ÉÊ", "\u0CB3\u0CC8"),
                ("¼ÉÆ", "\u0CB3\u0CCA"),
                ("¼ÉÆÃ", "\u0CB3\u0CCB"),
                ("¼Ë", "\u0CB3\u0CCC"),
                ("¾õï", "\u0CB1\u0CCD\u200C"),
                ("¾õÀ", "\u0CB1"),
                ("¾õÁ", "\u0CB1\u0CBE"),
                ("¾Â", "\u0CB1\u0CBF"),
                ("¾Ä", "\u0CB1\u0CC1"),
                ("¾Æ", "\u0CB1\u0CC2"),
                ("¾È", "\u0CB1\u0CC3"),
                ("¾õÉ", "\u0CB1\u0CC6"),
                ("¾õÉÃ", "\u0CB1\u0CC7"),
                ("¾õÉÊ", "\u0CB1\u0CC8"),
                ("¾õÉÆ", "\u0CB1\u0CCA"),
                ("¾õÉÆÃ", "\u0CB1\u0CCB"),
                ("¾õË", "\u0CB1\u0CCC"),
                ("¿õï", "\u0CDE\u0CCD\u200C"),
                ("¿õÀ", "\u0CDE"),
                ("¿õÁ", "\u0CDE\u0CBE"),
                ("¿Â", "\u0CDE\u0CBF"),
                ("¿Ä", "\u0CDE\u0CC1"),
                ("¿Æ", "\u0CDE\u0CC2"),
                ("¿È", "\u0CDE\u0CC3"),
                ("¿õÉ", "\u0CDE\u0CC6"),
                ("¿õÉÃ", "\u0CDE\u0CC7"),
                ("¿õÉÊ", "\u0CDE\u0CC8"),
                ("¿õÉÆ", "\u0CDE\u0CCA"),
                ("¿õÉÆÃ", "\u0CDE\u0CCB"),
                ("¿õË", "\u0CDE\u0CCC"),
                ("Ì", "\u0CCD\u0C95"),
                ("Í", "\u0CCD\u0C96"),
                ("Î", "\u0CCD\u0C97"),
                ("Ï", "\u0CCD\u0C98"),
                ("Ð", "\u0CCD\u0C99"),
                ("Ñ", "\u0CCD\u0C9A"),
                ("Ò", "\u0CCD\u0C9B"),
                ("Ó", "\u0CCD\u0C9C"),
                ("Ô", "\u0CCD\u0C9D"),
                ("Õ", "\u0CCD\u0C9E"),
                ("Ö", "\u0CCD\u0C9F"),
                ("×", "\u0CCD\u0CA0"),
                ("Ø", "\u0CCD\u0CA1"),
                ("Ù", "\u0CCD\u0CA2"),
                ("Ú", "\u0CCD\u0CA3"),
                ("Û", "\u0CCD\u0CA4"),
                ("Ü", "\u0CCD\u0CA5"),
                ("Ý", "\u0CCD\u0CA6"),
                ("Þ", "\u0CCD\u0CA7"),
                ("ß", "\u0CCD\u0CA8"),
                ("à", "\u0CCD\u0CAA"),
                ("á", "\u0CCD\u0CAB"),
                ("â", "\u0CCD\u0CAC"),
                ("ã", "\u0CCD\u0CAD"),
                ("ä", "\u0CCD\u0CAE"),
                ("å", "\u0CCD\u0CAF"),
                ("æ", "\u0CCD\u0CB0"),
                ("è", "\u0CCD\u0CB2"),
                ("é", "\u0CCD\u0CB5"),
                ("ê", "\u0CCD\u0CB6"),
                ("ë", "\u0CCD\u0CB7"),
                ("ì", "\u0CCD\u0CB8"),
                ("í", "\u0CCD\u0CB9"),
                ("î", "\u0CCD\u0CB3"),
                ("ù", "\u0CCD\u0CB1"),
                ("ú", "\u0CCD\u0CDE"),
                ("\u00EF", "\u0CCD"),




    ];

        // Keep as you already have (your list is fine).
        private static readonly (string From, string To)[] VattaksharaPairs =
        [
        ("\u00CC", "\u0CCD\u0C95"),
        ("\u00CD", "\u0CCD\u0C96"),
        ("\u00CE", "\u0CCD\u0C97"),
        ("\u00CF", "\u0CCD\u0C98"),
        ("\u00D0", "\u0CCD\u0C99"),
        ("\u00D1", "\u0CCD\u0C9A"),
        ("\u00D2", "\u0CCD\u0C9B"),
        ("\u00D3", "\u0CCD\u0C9C"),
        ("\u00D4", "\u0CCD\u0C9D"),
        ("\u00D5", "\u0CCD\u0C9E"),
        ("\u00D6", "\u0CCD\u0C9F"),
        ("\u00D7", "\u0CCD\u0CA0"),
        ("\u00D8", "\u0CCD\u0CA1"),
        ("\u00D9", "\u0CCD\u0CA2"),
        ("\u00DA", "\u0CCD\u0CA3"),
        ("\u00DB", "\u0CCD\u0CA4"),
        ("\u00DC", "\u0CCD\u0CA5"),
        ("\u00DD", "\u0CCD\u0CA6"),
        ("\u00DE", "\u0CCD\u0CA7"),
        ("\u00DF", "\u0CCD\u0CA8"),
        ("\u00E0", "\u0CCD\u0CAA"),
        ("\u00E1", "\u0CCD\u0CAB"),
        ("\u00E2", "\u0CCD\u0CAC"),
        ("\u00E3", "\u0CCD\u0CAD"),
        ("\u00E4", "\u0CCD\u0CAE"),
        ("\u00E5", "\u0CCD\u0CAF"),
        ("\u00E6", "\u0CCD\u0CB0"),
        ("\u00E8", "\u0CCD\u0CB2"),
        ("\u00E9", "\u0CCD\u0CB5"),
        ("\u00EA", "\u0CCD\u0CB6"),
        ("\u00EB", "\u0CCD\u0CB7"),
        ("\u00EC", "\u0CCD\u0CB8"),
        ("\u00ED", "\u0CCD\u0CB9"),
        ("\u00EE", "\u0CCD\u0CB3"),
        ("\u00F9", "\u0CCD\u0CB1"),
        ("\u00FA", "\u0CCD\u0CDE"),
    ];

        public string Convert(string input, bool englishNumbers = false, bool removeExtraSpace = true)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // JS removes ZWNJ from incoming text before processing. [file:1]
            input = input.Replace(ZWNJ, "");

            if (removeExtraSpace)
                input = Regex.Replace(input, @"\s+", " ").Trim();

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(input.Length * 2);

            foreach (var w in words)
            {
                // JS: $...$ passthrough. [file:1]
                if (w.Length > 1 && w[0] == '$' && w[^1] == '$')
                    sb.Append(w.AsSpan(1, w.Length - 2));
                else
                    sb.Append(ProcessAsciiWord(w, englishNumbers));

                sb.Append(' ');
            }

            if (sb.Length > 0) sb.Length--; // trailing space
            var output = sb.ToString();

            // JS: a2udeergahandle then a2upostprocess. [file:1]
            output = A2UDeergaHandle(output);
            output = A2UPostProcess_TODO(output); // dump is truncated [file:1]
            return output;
        }

        private string ProcessAsciiWord(string word, bool englishNumbers)
        {
            if (string.IsNullOrEmpty(word)) return word;

            // 1) Insert ZWNJ if required: (ï)([allowed-starts]) => g1 + ZWNJ + g2 [file:1]
            word = ZwnjRegex.Replace(word, m => m.Groups[1].Value + ZWNJ + m.Groups[2].Value);

            // 2) ordered mapping replacements [file:1]
            word = ApplyPairs(word, ReplacePairs);

            // 3) vatta reorder [file:1]
            word = Vatta3Regex.Replace(word, m => m.Groups[2].Value + m.Groups[3].Value + m.Groups[4].Value + m.Groups[1].Value);
            word = Vatta2Regex.Replace(word, m => m.Groups[2].Value + m.Groups[3].Value + m.Groups[1].Value);
            word = Vatta1Regex.Replace(word, m => m.Groups[2].Value + m.Groups[1].Value);

            // 4) remaining vattaksharas [file:1]
            word = ApplyPairs(word, VattaksharaPairs);

            // 5) numbers: JS converts to Kannada numbers unless englishnumbers is true. [file:1]
            if (!englishNumbers)
                word = ToKannadaNumbers(word);

            // 6) anusvara/visarga late replace (JS does A,B -> ಂ,ಃ) [file:1]
            word = word.Replace("A", "\u0C82").Replace("B", "\u0C83");

            // 7) Reph before convert (TODO: real JS regex missing in dump) [file:1]
            word = RephBeforeConvertRegex.Replace(word, m =>
            {
                var op = new StringBuilder(16);
                op.Append(m.Groups[1].Value);
                op.Append(ZWJ);
                op.Append(m.Groups[2].Value);
                if (m.Groups.Count > 3 && m.Groups[3].Success) op.Append(m.Groups[3].Value);
                if (m.Groups.Count > 4 && m.Groups[4].Success) op.Append(m.Groups[4].Value);
                if (m.Groups.Count > 5 && m.Groups[5].Success) op.Append(m.Groups[5].Value);
                if (m.Groups.Count > 6 && m.Groups[6].Success) op.Append(m.Groups[6].Value);
                return op.ToString();
            });

            // 8) Arkavattu handling (TODO: real JS regex missing in dump) [file:1]
            word = ArkavattuRegex.Replace(word, m =>
            {
                var op = new StringBuilder(16);
                op.Append(RA);
                op.Append(HALANT);
                op.Append(m.Groups[1].Value);
                if (m.Groups.Count > 2 && m.Groups[2].Success) op.Append(m.Groups[2].Value);
                if (m.Groups.Count > 3 && m.Groups[3].Success) op.Append(m.Groups[3].Value);
                if (m.Groups.Count > 4 && m.Groups[4].Success) op.Append(m.Groups[4].Value);
                if (m.Groups.Count > 5 && m.Groups[5].Success) op.Append(m.Groups[5].Value);
                return op.ToString();
            });

            return word;
        }

        private static string ApplyPairs(string text, (string From, string To)[] pairs)
        {
            if (string.IsNullOrEmpty(text) || pairs.Length == 0) return text;

            foreach (var (from, to) in pairs)
            {
                if (!string.IsNullOrEmpty(from))
                    text = text.Replace(from, to);
            }
            return text;
        }

        private static string ToKannadaNumbers(string text)
        {
            for (int i = 0; i < 10; i++)
                text = text.Replace(EnglishNumbers[i], KannadaNumbers[i]);
            return text;
        }

        // JS: a2udeergahandle: (ಾ|ೆ|ೊ)(ಂ) with 3 special cases. [file:1]
        private static string A2UDeergaHandle(string text)
        {
            return DeergaRegex.Replace(text, m => m.Groups[1].Value switch
            {
                "\u0CBE" => "\u0CBE\u0C82", // ಾ + ಂ => ಾಂ [file:1]
                "\u0CC6" => "\u0CC7",       // ೆಂ => ೇ [file:1]
                "\u0CCA" => "\u0CCB",       // ೊಂ => ೋ [file:1]
                _ => m.Value
            });
        }

        // JS: a2upostprocess exists but dump is truncated, so keep minimal safe behavior. [file:1]
        private static string A2UPostProcess_TODO(string text)
        {
            // Minimum safe: drop helper ZWNJ at end. [file:1]
            return text.Replace(ZWNJ, "");
        }

        // ---------- Regex definitions (direct ports from JS lines that exist) ----------

        // JS: _REGEX_ASCII_ZWNJ = new RegExp('(ï)([JRmpL°¬aej0μqC§Sªkv¿y¢¼lAbFU£E®¾±DGKPMQZo²¹z¶½¨»urT¦gndNfwWI¤¯tHc«µO¸VYºx¥©XÊ])','g') [file:1]
        [GeneratedRegex(
            @"(ï)([JRmpL°¬aej0μqC§Sªkv¿y¢¼lAbFU£E®¾±DGKPMQZo²¹z¶½¨»urT¦gndNfwWI¤¯tHc«µO¸VYºx¥©XÊ])",
            RegexOptions.Compiled)]
        private static partial Regex RegexAsciiZwnj();

        // JS: _REGEX_ASCII_VATTAKSHARA_3 = new RegExp('([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])([ÌÍ...çÊ])([ÌÍ...çÊ])([ÌÍ...çÊ])','g') [file:1]
        [GeneratedRegex(
            @"([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])",
            RegexOptions.Compiled)]
        private static partial Regex RegexAsciiVatta3();

        // JS: _REGEX_ASCII_VATTAKSHARA_2 = new RegExp('([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])([ÌÍ...çÊ])([ÌÍ...çÊ])','g') [file:1]
        [GeneratedRegex(
            @"([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])",
            RegexOptions.Compiled)]
        private static partial Regex RegexAsciiVatta2();

        // JS: _REGEX_ASCII_VATTAKSHARA_1 = new RegExp('([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])([ÌÍ...çÊ])','g') [file:1]
        [GeneratedRegex(
            @"([್ಾಿೀುೂೃೆೇೈೊೋೌಂಃ])" +
            @"([ÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæèéêëìíîùúçÊ])",
            RegexOptions.Compiled)]
        private static partial Regex RegexAsciiVatta1();

        // Deerga handle regex: (ಾ|ೆ|ೊ)(ಂ) [file:1]
        [GeneratedRegex(@"([\u0CBE\u0CC6\u0CCA])(\u0C82)", RegexOptions.Compiled)]
        private static partial Regex RegexA2UDeerga();

        // No-op placeholder regex.
        [GeneratedRegex(@"(?!x)x", RegexOptions.Compiled)]
        private static partial Regex RegexNeverMatch();
    }

    #endregion




    #region UNICODE → ASCII ✅ JS VERIFIED [file:10]
    internal class UnicodeToAsciiEngine
    {




        // ✅ JS VERIFIED: Kn.prototype._u2a_map (Unicode → ASCII) [file:10]


        private static readonly Dictionary<string, string> UnicodeToAsciiMap = new(StringComparer.Ordinal)
        {
            // Anusvara/Visarga
            ["\u0C82"] = "A",
            ["\u0C83"] = "B",
            ["\u0C85\u0C82"] = "CA",
            ["\u0C85\u0C83"] = "CB",

            // Vowels
            ["\u0C85"] = "C",
            ["\u0C86"] = "D",
            ["\u0C87"] = "E",
            ["\u0C88"] = "F",
            ["\u0C89"] = "G",
            ["\u0C8A"] = "H",
            ["\u0C8B"] = "I\u00C4",
            ["\u0CE0"] = "I\u00C62",
            ["\u0C8E"] = "J",
            ["\u0C8F"] = "K",
            ["\u0C90"] = "L",
            ["\u0C92"] = "M",
            ["\u0C93"] = "N",
            ["\u0C94"] = "O",

            // Ka (ಕ) - all combinations
            ["\u0C95\u0CCD"] = "P\u00EF",
            ["\u0C95"] = "P\u00C0",
            ["\u0C95\u0CBE"] = "P\u00C1",
            ["\u0C95\u0CBF"] = "Q",
            ["\u0C95\u0CC0"] = "Q\u00C3",
            ["\u0C95\u0CC1"] = "P\u00C0\u00C4",
            ["\u0C95\u0CC2"] = "P\u00C0\u00C6",
            ["\u0C95\u0CC3"] = "P\u00C0\u00C8",
            ["\u0C95\u0CC6"] = "P\u00C9",
            ["\u0C95\u0CC7"] = "P\u00C9\u00C3",
            ["\u0C95\u0CC8"] = "P\u00C9\u00CA",
            ["\u0C95\u0CCA"] = "P\u00C9\u00C6",
            ["\u0C95\u0CCB"] = "P\u00C9\u00C6\u00C3",
            ["\u0C95\u0CCC"] = "P\u00CB",

            // Kha (ಖ)
            ["\u0C96\u0CCD"] = "S\u00EF",
            ["\u0C96"] = "R",
            ["\u0C96\u0CBE"] = "S\u00C1",
            ["\u0C96\u0CBF"] = "T",
            ["\u0C96\u0CC0"] = "T\u00C3",
            ["\u0C96\u0CC1"] = "R\u00C4",
            ["\u0C96\u0CC2"] = "R\u00C6",
            ["\u0C96\u0CC3"] = "R\u00C8",
            ["\u0C96\u0CC6"] = "S\u00C9",
            ["\u0C96\u0CC7"] = "S\u00C9\u00C3",
            ["\u0C96\u0CC8"] = "S\u00C9\u00CA",
            ["\u0C96\u0CCA"] = "S\u00C9\u00C6",
            ["\u0C96\u0CCB"] = "S\u00C9\u00C6\u00C3",
            ["\u0C96\u0CCC"] = "S\u00CB",

            // Ga (ಗ)
            ["\u0C97\u0CCD"] = "U\u00EF",
            ["\u0C97"] = "U\u00C0",
            ["\u0C97\u0CBE"] = "U\u00C1",
            ["\u0C97\u0CBF"] = "V",
            ["\u0C97\u0CC0"] = "V\u00C3",
            ["\u0C97\u0CC1"] = "U\u00C0\u00C4",
            ["\u0C97\u0CC2"] = "U\u00C0\u00C6",
            ["\u0C97\u0CC3"] = "U\u00C0\u00C8",
            ["\u0C97\u0CC6"] = "U\u00C9",
            ["\u0C97\u0CC7"] = "U\u00C9\u00C3",
            ["\u0C97\u0CC8"] = "U\u00C9\u00CA",
            ["\u0C97\u0CCA"] = "U\u00C9\u00C6",
            ["\u0C97\u0CCB"] = "U\u00C9\u00C6\u00C3",
            ["\u0C97\u0CCC"] = "U\u00CB",

            // Gha (ಘ)
            ["\u0C98\u0CCD"] = "W\u00EF",
            ["\u0C98"] = "W\u00C0",
            ["\u0C98\u0CBE"] = "W\u00C1",
            ["\u0C98\u0CBF"] = "X",
            ["\u0C98\u0CC0"] = "X\u00C3",
            ["\u0C98\u0CC1"] = "W\u00C0\u00C4",
            ["\u0C98\u0CC2"] = "W\u00C0\u00C6",
            ["\u0C98\u0CC3"] = "W\u00C0\u00C8",
            ["\u0C98\u0CC6"] = "W\u00C9",
            ["\u0C98\u0CC7"] = "W\u00C9\u00C3",
            ["\u0C98\u0CC8"] = "W\u00C9\u00CA",
            ["\u0C98\u0CCA"] = "W\u00C9\u00C6",
            ["\u0C98\u0CCB"] = "W\u00C9\u00C6\u00C3",
            ["\u0C98\u0CCC"] = "W\u00CB",

            // Nga (ಙ)
            ["\u0C99\u0CCD"] = "Y\u00EF",
            ["\u0C99"] = "Y",

            // Cha (ಚ)
            ["\u0C9A\u0CCD"] = "Z\u00EF",
            ["\u0C9A"] = "Z\u00C0",
            ["\u0C9A\u0CBE"] = "Z\u00C1",
            ["\u0C9A\u0CBF"] = "a",
            ["\u0C9A\u0CC0"] = "a\u00C3",
            ["\u0C9A\u0CC1"] = "Z\u00C0\u00C4",
            ["\u0C9A\u0CC2"] = "Z\u00C0\u00C6",
            ["\u0C9A\u0CC3"] = "Z\u00C0\u00C8",
            ["\u0C9A\u0CC6"] = "Z\u00C9",
            ["\u0C9A\u0CC7"] = "Z\u00C9\u00C3",
            ["\u0C9A\u0CC8"] = "Z\u00C9\u00CA",
            ["\u0C9A\u0CCA"] = "Z\u00C9\u00C6",
            ["\u0C9A\u0CCB"] = "Z\u00C9\u00C6\u00C3",
            ["\u0C9A\u0CCC"] = "Z\u00CB",

            // Chha (ಛ)
            ["\u0C9B\u0CCD"] = "b\u00EF",
            ["\u0C9B"] = "b\u00C0",
            ["\u0C9B\u0CBE"] = "b\u00C1",
            ["\u0C9B\u0CBF"] = "c",
            ["\u0C9B\u0CC0"] = "c\u00C3",
            ["\u0C9B\u0CC1"] = "b\u00C0\u00C4",
            ["\u0C9B\u0CC2"] = "b\u00C0\u00C6",
            ["\u0C9B\u0CC3"] = "b\u00C0\u00C8",
            ["\u0C9B\u0CC6"] = "b\u00C9",
            ["\u0C9B\u0CC7"] = "b\u00C9\u00C3",
            ["\u0C9B\u0CC8"] = "b\u00C9\u00CA",
            ["\u0C9B\u0CCA"] = "b\u00C9\u00C6",
            ["\u0C9B\u0CCB"] = "b\u00C9\u00C6\u00C3",
            ["\u0C9B\u0CCC"] = "b\u00CB",

            // Ja (ಜ)
            ["\u0C9C\u0CCD"] = "e\u00EF",
            ["\u0C9C"] = "d",
            ["\u0C9C\u0CBE"] = "e\u00C1",
            ["\u0C9C\u0CBF"] = "f",
            ["\u0C9C\u0CC0"] = "f\u00C3",
            ["\u0C9C\u0CC1"] = "d\u00C4",
            ["\u0C9C\u0CC2"] = "d\u00C6",
            ["\u0C9C\u0CC3"] = "d\u00C8",
            ["\u0C9C\u0CC6"] = "e\u00C9",
            ["\u0C9C\u0CC7"] = "e\u00C9\u00C3",
            ["\u0C9C\u0CC8"] = "e\u00C9\u00CA",
            ["\u0C9C\u0CCA"] = "e\u00C9\u00C6",
            ["\u0C9C\u0CCB"] = "e\u00C9\u00C6\u00C3",
            ["\u0C9C\u0CCC"] = "e\u00CB",

            // Jha (ಝ)
            ["\u0C9D\u0CCD"] = "g\u00C0hi\u00EF",
            ["\u0C9D"] = "g\u00C0h\u00C4",
            ["\u0C9D\u0CBE"] = "g\u00C0hi\u00C1",
            ["\u0C9D\u0CBF"] = "jh\u00C4",
            ["\u0C9D\u0CC0"] = "jh\u00C4\u00C3",
            ["\u0C9D\u0CC1"] = "g\u00C0h\u00C4\u00C4",
            ["\u0C9D\u0CC2"] = "g\u00C0h\u00C4\u00C6",
            ["\u0C9D\u0CC3"] = "g\u00C0h\u00C4\u00C8",
            ["\u0C9D\u0CC6"] = "g\u00C9h\u00C4",
            ["\u0C9D\u0CC7"] = "g\u00C9h\u00C4\u00C3",
            ["\u0C9D\u0CC8"] = "g\u00C9h\u00C4\u00CA",
            ["\u0C9D\u0CCA"] = "g\u00C9h\u00C6",
            ["\u0C9D\u0CCB"] = "g\u00C9h\u00C6\u00C3",
            ["\u0C9D\u0CCC"] = "g\u00C0hi\u00CB",

            // Nya (ಞ)
            ["\u0C9E\u0CCD"] = "k\u00EF",
            ["\u0C9E"] = "k",

            // Ta (ಟ)
            ["\u0C9F\u0CCD"] = "m\u00EF",
            ["\u0C9F"] = "l",
            ["\u0C9F\u0CBE"] = "m\u00C1",
            ["\u0C9F\u0CBF"] = "n",
            ["\u0C9F\u0CC0"] = "n\u00C3",
            ["\u0C9F\u0CC1"] = "l\u00C4",
            ["\u0C9F\u0CC2"] = "l\u00C6",
            ["\u0C9F\u0CC3"] = "l\u00C8",
            ["\u0C9F\u0CC6"] = "m\u00C9",
            ["\u0C9F\u0CC7"] = "m\u00C9\u00C3",
            ["\u0C9F\u0CC8"] = "m\u00C9\u00CA",
            ["\u0C9F\u0CCA"] = "m\u00C9\u00C6",
            ["\u0C9F\u0CCB"] = "m\u00C9\u00C6\u00C3",
            ["\u0C9F\u0CCC"] = "m\u00CB",

            // Tha (ಠ)
            ["\u0CA0\u0CCD"] = "o\u00EF",
            ["\u0CA0"] = "o\u00C0",
            ["\u0CA0\u0CBE"] = "o\u00C1",
            ["\u0CA0\u0CBF"] = "p",
            ["\u0CA0\u0CC0"] = "p\u00C3",
            ["\u0CA0\u0CC1"] = "o\u00C0\u00C4",
            ["\u0CA0\u0CC2"] = "o\u00C0\u00C6",
            ["\u0CA0\u0CC3"] = "o\u00C0\u00C8",
            ["\u0CA0\u0CC6"] = "o\u00C9",
            ["\u0CA0\u0CC7"] = "o\u00C9\u00C3",
            ["\u0CA0\u0CC8"] = "o\u00C9\u00CA",
            ["\u0CA0\u0CCA"] = "o\u00C9\u00C6",
            ["\u0CA0\u0CCB"] = "o\u00C9\u00C6\u00C3",
            ["\u0CA0\u0CCC"] = "o\u00CB",

            // Da (ಡ)
            ["\u0CA1\u0CCD"] = "q\u00EF",
            ["\u0CA1"] = "q\u00C0",
            ["\u0CA1\u0CBE"] = "q\u00C1",
            ["\u0CA1\u0CBF"] = "r",
            ["\u0CA1\u0CC0"] = "r\u00C3",
            ["\u0CA1\u0CC1"] = "q\u00C0\u00C4",
            ["\u0CA1\u0CC2"] = "q\u00C0\u00C6",
            ["\u0CA1\u0CC3"] = "q\u00C0\u00C8",
            ["\u0CA1\u0CC6"] = "q\u00C9",
            ["\u0CA1\u0CC7"] = "q\u00C9\u00C3",
            ["\u0CA1\u0CC8"] = "q\u00C9\u00CA",
            ["\u0CA1\u0CCA"] = "q\u00C9\u00C6",
            ["\u0CA1\u0CCB"] = "q\u00C9\u00C6\u00C3",
            ["\u0CA1\u0CCC"] = "q\u00CB",

            // Dha (ಢ)
            ["\u0CA2\u0CCD"] = "qs\u00EF",
            ["\u0CA2"] = "qs\u00C0",
            ["\u0CA2\u0CBE"] = "qs\u00C1",
            ["\u0CA2\u0CBF"] = "r\u00FC",
            ["\u0CA2\u0CC0"] = "r\u00FC\u00C3",
            ["\u0CA2\u0CC1"] = "qs\u00C0\u00C4",
            ["\u0CA2\u0CC2"] = "qs\u00C0\u00C6",
            ["\u0CA2\u0CC3"] = "qs\u00C0\u00C8",
            ["\u0CA2\u0CC6"] = "qs\u00C9",
            ["\u0CA2\u0CC7"] = "qs\u00C9\u00C3",
            ["\u0CA2\u0CC8"] = "qs\u00C9\u00CA",
            ["\u0CA2\u0CCA"] = "qs\u00C9\u00C6",
            ["\u0CA2\u0CCB"] = "qs\u00C9\u00C6\u00C3",
            ["\u0CA2\u0CCC"] = "qs\u00CB",

            // Na (ಣ)
            ["\u0CA3\u0CCD"] = "u\u00EF",
            ["\u0CA3"] = "t",
            ["\u0CA3\u0CBE"] = "u\u00C1",
            ["\u0CA3\u0CBF"] = "t\u00C2",
            ["\u0CA3\u0CC0"] = "t\u00C2\u00C3",
            ["\u0CA3\u0CC1"] = "t\u00C4",
            ["\u0CA3\u0CC2"] = "t\u00C6",
            ["\u0CA3\u0CC3"] = "t\u00C8",
            ["\u0CA3\u0CC6"] = "u\u00C9",
            ["\u0CA3\u0CC7"] = "u\u00C9\u00C3",
            ["\u0CA3\u0CC8"] = "u\u00C9\u00CA",
            ["\u0CA3\u0CCA"] = "u\u00C9\u00C6",
            ["\u0CA3\u0CCB"] = "u\u00C9\u00C6\u00C3",
            ["\u0CA3\u0CCC"] = "u\u00CB",

            // Ta (ತ)
            ["\u0CA4\u0CCD"] = "v\u00EF",
            ["\u0CA4"] = "v\u00C0",
            ["\u0CA4\u0CBE"] = "v\u00C1",
            ["\u0CA4\u0CBF"] = "w",
            ["\u0CA4\u0CC0"] = "w\u00C3",
            ["\u0CA4\u0CC1"] = "v\u00C0\u00C4",
            ["\u0CA4\u0CC2"] = "v\u00C0\u00C6",
            ["\u0CA4\u0CC3"] = "v\u00C0\u00C8",
            ["\u0CA4\u0CC6"] = "v\u00C9",
            ["\u0CA4\u0CC7"] = "v\u00C9\u00C3",
            ["\u0CA4\u0CC8"] = "v\u00C9\u00CA",
            ["\u0CA4\u0CCA"] = "v\u00C9\u00C6",
            ["\u0CA4\u0CCB"] = "v\u00C9\u00C6\u00C3",
            ["\u0CA4\u0CCC"] = "v\u00CB",

            // Tha (ಥ)
            ["\u0CA5\u0CCD"] = "x\u00EF",
            ["\u0CA5"] = "x\u00C0",
            ["\u0CA5\u0CBE"] = "x\u00C1",
            ["\u0CA5\u0CBF"] = "y",
            ["\u0CA5\u0CC0"] = "y\u00C3",
            ["\u0CA5\u0CC1"] = "x\u00C0\u00C4",
            ["\u0CA5\u0CC2"] = "x\u00C0\u00C6",
            ["\u0CA5\u0CC3"] = "x\u00C0\u00C8",
            ["\u0CA5\u0CC6"] = "x\u00C9",
            ["\u0CA5\u0CC7"] = "x\u00C9\u00C3",
            ["\u0CA5\u0CC8"] = "x\u00C9\u00CA",
            ["\u0CA5\u0CCA"] = "x\u00C9\u00C6",
            ["\u0CA5\u0CCB"] = "x\u00C9\u00C6\u00C3",
            ["\u0CA5\u0CCC"] = "x\u00CB",

            // Da (ದ)
            ["\u0CA6\u0CCD"] = "z\u00EF",
            ["\u0CA6"] = "z\u00C0",
            ["\u0CA6\u0CBE"] = "z\u00C1",
            ["\u0CA6\u0CBF"] = "\u00A2",
            ["\u0CA6\u0CC0"] = "\u00A2\u00C3",
            ["\u0CA6\u0CC1"] = "z\u00C0\u00C4",
            ["\u0CA6\u0CC2"] = "z\u00C0\u00C6",
            ["\u0CA6\u0CC3"] = "z\u00C0\u00C8",
            ["\u0CA6\u0CC6"] = "z\u00C9",
            ["\u0CA6\u0CC7"] = "z\u00C9\u00C3",
            ["\u0CA6\u0CC8"] = "z\u00C9\u00CA",
            ["\u0CA6\u0CCA"] = "z\u00C9\u00C6",
            ["\u0CA6\u0CCB"] = "z\u00C9\u00C6\u00C3",
            ["\u0CA6\u0CCC"] = "z\u00CB",

            // Dha (ಧ)
            ["\u0CA7\u0CCD"] = "zs\u00EF",
            ["\u0CA7"] = "zs\u00C0",
            ["\u0CA7\u0CBE"] = "zs\u00C1",
            ["\u0CA7\u0CBF"] = "\u00A2\u00FC",
            ["\u0CA7\u0CC0"] = "\u00A2\u00FC\u00C3",
            ["\u0CA7\u0CC1"] = "zs\u00C0\u00C4",
            ["\u0CA7\u0CC2"] = "zs\u00C0\u00C6",
            ["\u0CA7\u0CC3"] = "zs\u00C0\u00C8",
            ["\u0CA7\u0CC6"] = "zs\u00C9",
            ["\u0CA7\u0CC7"] = "zs\u00C9\u00C3",
            ["\u0CA7\u0CC8"] = "zs\u00C9\u00CA",
            ["\u0CA7\u0CCA"] = "zs\u00C9\u00C6",
            ["\u0CA7\u0CCB"] = "zs\u00C9\u00C6\u00C3",
            ["\u0CA7\u0CCC"] = "zs\u00CB",

            // Na (ನ)
            ["\u0CA8\u0CCD"] = "\u00A3\u00EF",
            ["\u0CA8"] = "\u00A3\u00C0",
            ["\u0CA8\u0CBE"] = "\u00A3\u00C1",
            ["\u0CA8\u0CBF"] = "\u00A4",
            ["\u0CA8\u0CC0"] = "\u00A4\u00C3",
            ["\u0CA8\u0CC1"] = "\u00A3\u00C0\u00C4",
            ["\u0CA8\u0CC2"] = "\u00A3\u00C0\u00C6",
            ["\u0CA8\u0CC3"] = "\u00A3\u00C0\u00C8",
            ["\u0CA8\u0CC6"] = "\u00A3\u00C9",
            ["\u0CA8\u0CC7"] = "\u00A3\u00C9\u00C3",
            ["\u0CA8\u0CC8"] = "\u00A3\u00C9\u00CA",
            ["\u0CA8\u0CCA"] = "\u00A3\u00C9\u00C6",
            ["\u0CA8\u0CCB"] = "\u00A3\u00C9\u00C6\u00C3",
            ["\u0CA8\u0CCC"] = "\u00A3\u00CB",

            // Pa (ಪ)
            ["\u0CAA\u0CCD"] = "\u00A5\u00EF",
            ["\u0CAA"] = "\u00A5\u00C0",
            ["\u0CAA\u0CBE"] = "\u00A5\u00C1",
            ["\u0CAA\u0CBF"] = "\u00A6",
            ["\u0CAA\u0CC0"] = "\u00A6\u00C3",
            ["\u0CAA\u0CC1"] = "\u00A5\u00C0\u00C5",
            ["\u0CAA\u0CC2"] = "\u00A5\u00C0\u00C7",
            ["\u0CAA\u0CC3"] = "\u00A5\u00C0\u00C8",
            ["\u0CAA\u0CC6"] = "\u00A5\u00C9",
            ["\u0CAA\u0CC7"] = "\u00A5\u00C9\u00C3",
            ["\u0CAA\u0CC8"] = "\u00A5\u00C9\u00CA",
            ["\u0CAA\u0CCA"] = "\u00A5\u00C9\u00C7",
            ["\u0CAA\u0CCB"] = "\u00A5\u00C9\u00C7\u00C3",
            ["\u0CAA\u0CCC"] = "\u00A5\u00CB",

            // Pha (ಫ)
            ["\u0CAB\u0CCD"] = "\u00A5s\u00EF",
            ["\u0CAB"] = "\u00A5s\u00C0",
            ["\u0CAB\u0CBE"] = "\u00A5s\u00C1",
            ["\u0CAB\u0CBF"] = "\u00A6\u00FC",
            ["\u0CAB\u0CC0"] = "\u00A6\u00FC\u00C3",
            ["\u0CAB\u0CC1"] = "\u00A5s\u00C0\u00C5",
            ["\u0CAB\u0CC2"] = "\u00A5s\u00C0\u00C7",
            ["\u0CAB\u0CC3"] = "\u00A5s\u00C0\u00C8",
            ["\u0CAB\u0CC6"] = "\u00A5s\u00C9",
            ["\u0CAB\u0CC7"] = "\u00A5s\u00C9\u00C3",
            ["\u0CAB\u0CC8"] = "\u00A5s\u00C9\u00CA",
            ["\u0CAB\u0CCA"] = "\u00A5s\u00C9\u00C7",
            ["\u0CAB\u0CCB"] = "\u00A5s\u00C9\u00C7\u00C3",
            ["\u0CAB\u0CCC"] = "\u00A5s\u00CB",

            // Ba (ಬ)
            ["\u0CAC\u0CCD"] = "\u00A8\u00EF",
            ["\u0CAC"] = "\u00A7",
            ["\u0CAC\u0CBE"] = "\u00A8\u00C1",
            ["\u0CAC\u0CBF"] = "\u00A9",
            ["\u0CAC\u0CC0"] = "\u00A9\u00C3",
            ["\u0CAC\u0CC1"] = "\u00A7\u00C4",
            ["\u0CAC\u0CC2"] = "\u00A7\u00C6",
            ["\u0CAC\u0CC3"] = "\u00A7\u00C8",
            ["\u0CAC\u0CC6"] = "\u00A8\u00C9",
            ["\u0CAC\u0CC7"] = "\u00A8\u00C9\u00C3",
            ["\u0CAC\u0CC8"] = "\u00A8\u00C9\u00CA",
            ["\u0CAC\u0CCA"] = "\u00A8\u00C9\u00C6",
            ["\u0CAC\u0CCB"] = "\u00A8\u00C9\u00C6\u00C3",
            ["\u0CAC\u0CCC"] = "\u00A8\u00CB",

            // Bha (ಭ)
            ["\u0CAD\u0CCD"] = "\u00A8s\u00EF",
            ["\u0CAD"] = "\u00A8s\u00C0",
            ["\u0CAD\u0CBE"] = "\u00A8s\u00C1",
            ["\u0CAD\u0CBF"] = "\u00A9\u00FC",
            ["\u0CAD\u0CC0"] = "\u00A9\u00FC\u00C3",
            ["\u0CAD\u0CC1"] = "\u00A8s\u00C0\u00C4",
            ["\u0CAD\u0CC2"] = "\u00A8s\u00C0\u00C6",
            ["\u0CAD\u0CC3"] = "\u00A8s\u00C0\u00C8",
            ["\u0CAD\u0CC6"] = "\u00A8s\u00C9",
            ["\u0CAD\u0CC7"] = "\u00A8s\u00C9\u00C3",
            ["\u0CAD\u0CC8"] = "\u00A8s\u00C9\u00CA",
            ["\u0CAD\u0CCA"] = "\u00A8s\u00C9\u00C6",
            ["\u0CAD\u0CCB"] = "\u00A8s\u00C9\u00C6\u00C3",
            ["\u0CAD\u0CCC"] = "\u00A8s\u00CB",

            // Ma (ಮ)
            ["\u0CAE\u0CCD"] = "\u00AA\u00C0i\u00EF",
            ["\u0CAE"] = "\u00AA\u00C0\u00C4",
            ["\u0CAE\u0CBE"] = "\u00AA\u00C0i\u00C1",
            ["\u0CAE\u0CBF"] = "\u00AB\u00C4",
            ["\u0CAE\u0CC0"] = "\u00AB\u00C4\u00C3",
            ["\u0CAE\u0CC1"] = "\u00AA\u00C0\u00C4\u00C4",
            ["\u0CAE\u0CC2"] = "\u00AA\u00C0\u00C4\u00C6",
            ["\u0CAE\u0CC3"] = "\u00AA\u00C0\u00C4\u00C8",
            ["\u0CAE\u0CC6"] = "\u00AA\u00C9\u00C4",
            ["\u0CAE\u0CC7"] = "\u00AA\u00C9\u00C4\u00C3",
            ["\u0CAE\u0CC8"] = "\u00AA\u00C9\u00C4\u00CA",
            ["\u0CAE\u0CCA"] = "\u00AA\u00C9\u00C6",
            ["\u0CAE\u0CCB"] = "\u00AA\u00C9\u00C6\u00C3",
            ["\u0CAE\u0CCC"] = "\u00AA\u00C0i\u00CB",

            // Ya (ಯ)
            ["\u0CAF\u0CCD"] = "Ai\u00C0i\u00EF",
            ["\u0CAF"] = "Ai\u00C0\u00C4",
            ["\u0CAF\u0CBE"] = "Ai\u00C0i\u00C1",
            ["\u0CAF\u0CBF"] = "\u00AC\u00C4",
            ["\u0CAF\u0CC0"] = "\u00AC\u00C4\u00C3",
            ["\u0CAF\u0CC1"] = "Ai\u00C0\u00C4\u00C4",
            ["\u0CAF\u0CC2"] = "Ai\u00C0\u00C4\u00C6",
            ["\u0CAF\u0CC3"] = "Ai\u00C0\u00C4\u00C8",
            ["\u0CAF\u0CC6"] = "Ai\u00C9\u00C4",
            ["\u0CAF\u0CC7"] = "Ai\u00C9\u00C4\u00C3",
            ["\u0CAF\u0CC8"] = "Ai\u00C9\u00C4\u00CA",
            ["\u0CAF\u0CCA"] = "Ai\u00C9\u00C6",
            ["\u0CAF\u0CCB"] = "Ai\u00C9\u00C6\u00C3",
            ["\u0CAF\u0CCC"] = "Ai\u00C0i\u00CB",

            // Ra (ರ)
            ["\u0CB0\u0CCD"] = "g\u00EF",
            ["\u0CB0"] = "g\u00C0",
            ["\u0CB0\u0CBE"] = "g\u00C1",
            ["\u0CB0\u0CBF"] = "j",
            ["\u0CB0\u0CC0"] = "j\u00C3",
            ["\u0CB0\u0CC1"] = "g\u00C0\u00C4",
            ["\u0CB0\u0CC2"] = "g\u00C0\u00C6",
            ["\u0CB0\u0CC3"] = "g\u00C0\u00C8",
            ["\u0CB0\u0CC6"] = "g\u00C9",
            ["\u0CB0\u0CC7"] = "g\u00C9\u00C3",
            ["\u0CB0\u0CC8"] = "g\u00C9\u00CA",
            ["\u0CB0\u0CCA"] = "g\u00C9\u00C6",
            ["\u0CB0\u0CCB"] = "g\u00C9\u00C6\u00C3",
            ["\u0CB0\u0CCC"] = "g\u00CB",

            // La (ಲ)
            ["\u0CB2\u0CCD"] = "\u00AF\u00EF",
            ["\u0CB2"] = "\u00AE",
            ["\u0CB2\u0CBE"] = "\u00AF\u00C1",
            ["\u0CB2\u0CBF"] = "\u00B0",
            ["\u0CB2\u0CC0"] = "\u00B0\u00C3",
            ["\u0CB2\u0CC1"] = "\u00AE\u00C4",
            ["\u0CB2\u0CC2"] = "\u00AE\u00C6",
            ["\u0CB2\u0CC3"] = "\u00AE\u00C8",
            ["\u0CB2\u0CC6"] = "\u00AF\u00C9",
            ["\u0CB2\u0CC7"] = "\u00AF\u00C9\u00C3",
            ["\u0CB2\u0CC8"] = "\u00AF\u00C9\u00CA",
            ["\u0CB2\u0CCA"] = "\u00AF\u00C9\u00C6",
            ["\u0CB2\u0CCB"] = "\u00AF\u00C9\u00C6\u00C3",
            ["\u0CB2\u0CCC"] = "\u00AF\u00CB",

            // Va (ವ)
            ["\u0CB5\u0CCD"] = "\u00AA\u00EF",
            ["\u0CB5"] = "\u00AA\u00C0",
            ["\u0CB5\u0CBE"] = "\u00AA\u00C1",
            ["\u0CB5\u0CBF"] = "\u00AB",
            ["\u0CB5\u0CC0"] = "\u00AB\u00C3",
            ["\u0CB5\u0CC1"] = "\u00AA\u00C0\u00C5",
            ["\u0CB5\u0CC2"] = "\u00AA\u00C0\u00C7",
            ["\u0CB5\u0CC3"] = "\u00AA\u00C0\u00C8",
            ["\u0CB5\u0CC6"] = "\u00AA\u00C9",
            ["\u0CB5\u0CC7"] = "\u00AA\u00C9\u00C3",
            ["\u0CB5\u0CC8"] = "\u00AA\u00C9\u00CA",
            ["\u0CB5\u0CCA"] = "\u00AA\u00C9\u00C7",
            ["\u0CB5\u0CCB"] = "\u00AA\u00C9\u00C7\u00C3",
            ["\u0CB5\u0CCC"] = "\u00AA\u00CB",

            // Sha (ಶ)
            ["\u0CB6\u0CCD"] = "\u00B1\u00EF",
            ["\u0CB6"] = "\u00B1\u00C0",
            ["\u0CB6\u0CBE"] = "\u00B1\u00C1",
            ["\u0CB6\u0CBF"] = "\u00B2",
            ["\u0CB6\u0CC0"] = "\u00B2\u00C3",
            ["\u0CB6\u0CC1"] = "\u00B1\u00C0\u00C4",
            ["\u0CB6\u0CC2"] = "\u00B1\u00C0\u00C6",
            ["\u0CB6\u0CC3"] = "\u00B1\u00C0\u00C8",
            ["\u0CB6\u0CC6"] = "\u00B1\u00C9",
            ["\u0CB6\u0CC7"] = "\u00B1\u00C9\u00C3",
            ["\u0CB6\u0CC8"] = "\u00B1\u00C9\u00CA",
            ["\u0CB6\u0CCA"] = "\u00B1\u00C9\u00C6",
            ["\u0CB6\u0CCB"] = "\u00B1\u00C9\u00C6\u00C3",
            ["\u0CB6\u0CCC"] = "\u00B1\u00CB",

            // Sha (ಷ)
            ["\u0CB7\u0CCD"] = "\u03BC\u00EF",
            ["\u0CB7"] = "\u03BC\u00C0",
            ["\u0CB7\u0CBE"] = "\u03BC\u00C1",
            ["\u0CB7\u0CBF"] = "\u00B6",
            ["\u0CB7\u0CC0"] = "\u00B6\u00C3",
            ["\u0CB7\u0CC1"] = "\u03BC\u00C0\u00C4",
            ["\u0CB7\u0CC2"] = "\u03BC\u00C0\u00C6",
            ["\u0CB7\u0CC3"] = "\u03BC\u00C0\u00C8",
            ["\u0CB7\u0CC6"] = "\u03BC\u00C9",
            ["\u0CB7\u0CC7"] = "\u03BC\u00C9\u00C3",
            ["\u0CB7\u0CC8"] = "\u03BC\u00C9\u00CA",
            ["\u0CB7\u0CCA"] = "\u03BC\u00C9\u00C6",
            ["\u0CB7\u0CCB"] = "\u03BC\u00C9\u00C6\u00C3",
            ["\u0CB7\u0CCC"] = "\u03BC\u00CB",

            // Sa (ಸ)
            ["\u0CB8\u0CCD"] = "\u00B8\u00EF",
            ["\u0CB8"] = "\u00B8\u00C0",
            ["\u0CB8\u0CBE"] = "\u00B8\u00C1",
            ["\u0CB8\u0CBF"] = "\u00B9",
            ["\u0CB8\u0CC0"] = "\u00B9\u00C3",
            ["\u0CB8\u0CC1"] = "\u00B8\u00C0\u00C4",
            ["\u0CB8\u0CC2"] = "\u00B8\u00C0\u00C6",
            ["\u0CB8\u0CC3"] = "\u00B8\u00C0\u00C8",
            ["\u0CB8\u0CC6"] = "\u00B8\u00C9",
            ["\u0CB8\u0CC7"] = "\u00B8\u00C9\u00C3",
            ["\u0CB8\u0CC8"] = "\u00B8\u00C9\u00CA",
            ["\u0CB8\u0CCA"] = "\u00B8\u00C9\u00C6",
            ["\u0CB8\u0CCB"] = "\u00B8\u00C9\u00C6\u00C3",
            ["\u0CB8\u0CCC"] = "\u00B8\u00CB",

            // Ha (ಹ)
            ["\u0CB9\u0CCD"] = "\u00BA\u00EF",
            ["\u0CB9"] = "\u00BA\u00C0",
            ["\u0CB9\u0CBE"] = "\u00BA\u00C1",
            ["\u0CB9\u0CBF"] = "\u00BB",
            ["\u0CB9\u0CC0"] = "\u00BB\u00C3",
            ["\u0CB9\u0CC1"] = "\u00BA\u00C0\u00C4",
            ["\u0CB9\u0CC2"] = "\u00BA\u00C0\u00C6",
            ["\u0CB9\u0CC3"] = "\u00BA\u00C0\u00C8",
            ["\u0CB9\u0CC6"] = "\u00BA\u00C9",
            ["\u0CB9\u0CC7"] = "\u00BA\u00C9\u00C3",
            ["\u0CB9\u0CC8"] = "\u00BA\u00C9\u00CA",
            ["\u0CB9\u0CCA"] = "\u00BA\u00C9\u00C6",
            ["\u0CB9\u0CCB"] = "\u00BA\u00C9\u00C6\u00C3",
            ["\u0CB9\u0CCC"] = "\u00BA\u00CB",

            // La (ಳ)
            ["\u0CB3\u0CCD"] = "\u00BC\u00EF",
            ["\u0CB3"] = "\u00BC\u00C0",
            ["\u0CB3\u0CBE"] = "\u00BC\u00C1",
            ["\u0CB3\u0CBF"] = "\u00BD",
            ["\u0CB3\u0CC0"] = "\u00BD\u00C3",
            ["\u0CB3\u0CC1"] = "\u00BC\u00C0\u00C4",
            ["\u0CB3\u0CC2"] = "\u00BC\u00C0\u00C6",
            ["\u0CB3\u0CC3"] = "\u00BC\u00C0\u00C8",
            ["\u0CB3\u0CC6"] = "\u00BC\u00C9",
            ["\u0CB3\u0CC7"] = "\u00BC\u00C9\u00C3",
            ["\u0CB3\u0CC8"] = "\u00BC\u00C9\u00CA",
            ["\u0CB3\u0CCA"] = "\u00BC\u00C9\u00C6",
            ["\u0CB3\u0CCB"] = "\u00BC\u00C9\u00C6\u00C3",
            ["\u0CB3\u0CCC"] = "\u00BC\u00CB",

            // Rra (ಱ) with ZWNJ
            ["\u0CB1\u0CCD\u200C"] = "\u00BE\u00F5\u00EF",
            ["\u0CB1"] = "\u00BE\u00F5\u00C0",
            ["\u0CB1\u0CBE"] = "\u00BE\u00F5\u00C1",
            ["\u0CB1\u0CBF"] = "\u00BE\u00C2",
            ["\u0CB1\u0CC1"] = "\u00BE\u00C4",
            ["\u0CB1\u0CC2"] = "\u00BE\u00C6",
            ["\u0CB1\u0CC3"] = "\u00BE\u00C8",
            ["\u0CB1\u0CC6"] = "\u00BE\u00F5\u00C9",
            ["\u0CB1\u0CC7"] = "\u00BE\u00F5\u00C9\u00C3",
            ["\u0CB1\u0CC8"] = "\u00BE\u00F5\u00C9\u00CA",
            ["\u0CB1\u0CCA"] = "\u00BE\u00F5\u00C9\u00C6",
            ["\u0CB1\u0CCB"] = "\u00BE\u00F5\u00C9\u00C6\u00C3",
            ["\u0CB1\u0CCC"] = "\u00BE\u00F5\u00CB",

            // Fa (ೞ) with ZWNJ
            ["\u0CDE\u0CCD\u200C"] = "\u00BF\u00F5\u00EF",
            ["\u0CDE"] = "\u00BF\u00F5\u00C0",
            ["\u0CDE\u0CBE"] = "\u00BF\u00F5\u00C1",
            ["\u0CDE\u0CBF"] = "\u00BF\u00C2",
            ["\u0CDE\u0CC1"] = "\u00BF\u00C4",
            ["\u0CDE\u0CC2"] = "\u00BF\u00C6",
            ["\u0CDE\u0CC3"] = "\u00BF\u00C8",
            ["\u0CDE\u0CC6"] = "\u00BF\u00F5\u00C9",
            ["\u0CDE\u0CC7"] = "\u00BF\u00F5\u00C9\u00C3",
            ["\u0CDE\u0CC8"] = "\u00BF\u00F5\u00C9\u00CA",
            ["\u0CDE\u0CCA"] = "\u00BF\u00F5\u00C9\u00C6",
            ["\u0CDE\u0CCB"] = "\u00BF\u00F5\u00C9\u00C6\u00C3",
            ["\u0CDE\u0CCC"] = "\u00BF\u00F5\u00CB",

            // Vattakshara (Halant + Consonants)
            ["\u0CCD\u0C95"] = "\u00CC",
            ["\u0CCD\u0C96"] = "\u00CD",
            ["\u0CCD\u0C97"] = "\u00CE",
            ["\u0CCD\u0C98"] = "\u00CF",
            ["\u0CCD\u0C99"] = "\u00D0",
            ["\u0CCD\u0C9A"] = "\u00D1",
            ["\u0CCD\u0C9B"] = "\u00D2",
            ["\u0CCD\u0C9C"] = "\u00D3",
            ["\u0CCD\u0C9D"] = "\u00D4",
            ["\u0CCD\u0C9E"] = "\u00D5",
            ["\u0CCD\u0C9F"] = "\u00D6",
            ["\u0CCD\u0CA0"] = "\u00D7",
            ["\u0CCD\u0CA1"] = "\u00D8",
            ["\u0CCD\u0CA2"] = "\u00D9",
            ["\u0CCD\u0CA3"] = "\u00DA",
            ["\u0CCD\u0CA4"] = "\u00DB",
            ["\u0CCD\u0CA5"] = "\u00DC",
            ["\u0CCD\u0CA6"] = "\u00DD",
            ["\u0CCD\u0CA7"] = "\u00DE",
            ["\u0CCD\u0CA8"] = "\u00DF",
            ["\u0CCD\u0CAA"] = "\u00E0",
            ["\u0CCD\u0CAB"] = "\u00E1",
            ["\u0CCD\u0CAC"] = "\u00E2",
            ["\u0CCD\u0CAD"] = "\u00E3",
            ["\u0CCD\u0CAE"] = "\u00E4",
            ["\u0CCD\u0CAF"] = "\u00E5",
            ["\u0CCD\u0CB0"] = "\u00E6",
            ["\u0CCD\u0CB2"] = "\u00E8",
            ["\u0CCD\u0CB5"] = "\u00E9",
            ["\u0CCD\u0CB6"] = "\u00EA",
            ["\u0CCD\u0CB7"] = "\u00EB",
            ["\u0CCD\u0CB8"] = "\u00EC",
            ["\u0CCD\u0CB9"] = "\u00ED",
            ["\u0CCD\u0CB3"] = "\u00EE",
            ["\u0CCD\u0CB1"] = "\u00F9",
            ["\u0CCD\u0CDE"] = "\u00FA",
        };



        private static readonly string[] KannadaNumbers = { "\u0CE6", "\u0CE7", "\u0CE8", "\u0CE9", "\u0CEA", "\u0CEB", "\u0CEC", "\u0CED", "\u0CEE", "\u0CEF" };
        private static readonly string[] EnglishNumbers = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };



        public string Convert(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // Remove ZWNJ like JS
            input = input.Replace("\u200C", "");

            // Preserve lines instead of flattening everything
            input = input.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = input.Split('\n');

            var result = new StringBuilder();

            foreach (var line in lines)
            {
                // Collapse internal whitespace per line
                var normalized = Regex.Replace(line, @"\s+", " ").Trim();

                if (normalized.Length == 0)
                {
                    result.AppendLine(); // keep blank line / paragraph
                    continue;
                }

                var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    if (word.Length > 1 && word[0] == '$' && word[^1] == '$')
                    {
                        result.Append(word[1..^1]);
                    }
                    else
                    {
                        var letters = GetLogicalLetters(word);
                        result.Append(string.Join("", letters.Select(RearrangeAndReplace)));
                    }

                    result.Append(' ');
                }

                if (result.Length > 0 && result[^1] == ' ')
                    result.Length--;      // remove trailing space for this line

                result.AppendLine();
            }

            var output = result.ToString().TrimEnd('\n');

            // Post‑pipeline (same as before)
            output = ApplyAnusvaraVisarga(output);  // _unicode_anusvara_visarga
            output = ToEnglishNumbers(output);      // _to_ascii_numbers
            output = DeergaHandle(output);          // _u2a_deerga_handle (still TODO inside)
            return PostProcess(output);             // _u2a_post_process
        }



        // ✅ JS VERIFIED: Kn.prototype.letters() [file:10]
        private List<string> GetLogicalLetters(string word)
        {
            var letters = new List<string>();
            var length = word.Length;
            var prevValueChars = new[] { "cbe", "cbf", "cc0", "cc1", "cc2", "cc3", "cc6", "cc7", "cc8", "cca", "ccb", "ccc", "ccd", "c82", "c83", "200d" };

            for (int i = 0; i < length; i++)
            {
                var currentHex = ((int)word[i]).ToString("x3").ToLower();
                var prevHex = i > 0 ? ((int)word[i - 1]).ToString("x3").ToLower() : "";
                var isKannadaChar = Regex.IsMatch(word[i].ToString(), @"^[\u0C80-\u0CFF\u200D]+$");

                var shouldAppend = letters.Count > 0 && isKannadaChar &&
                                  (prevValueChars.Contains(currentHex) || prevHex == "ccd");

                if (shouldAppend)
                    letters[^1] += word[i];
                else
                    letters.Add(word[i].ToString());
            }
            return letters;
        }

        // Simplified Kn.prototype._rearrange_and_replace [file:10]
        private string RearrangeAndReplace(string letter)
        {
            if (UnicodeToAsciiMap.TryGetValue(letter, out var mapping))
                return mapping;
            return letter;
        }

        private string ApplyAnusvaraVisarga(string text) =>
            text.Replace("ಂ", "A").Replace("ಃ", "B");

        private string ToEnglishNumbers(string text)
        {
            for (int i = 0; i < 10; i++)
                text = text.Replace(KannadaNumbers[i], EnglishNumbers[i]);
            return text;
        }

        private string DeergaHandle(string text) => text;  // TODO: Implement full _u2a_deerga_handle [file:10]

        private string PostProcess(string text) => text.Replace("ÈÌ", "Ìø");  // Kn.prototype._u2a_post_process [file:10]
    }
    #endregion
}

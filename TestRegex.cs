using System;
using System.Text.RegularExpressions;
using System.Net;

public class Program
{
    public static void Main()
    {
        string html = "<p>Hello <b>World</b></p>";
        var parts = Regex.Split(html, @"(<[^>]+>)");
        foreach (var part in parts)
        {
            if (part.StartsWith("<") && part.EndsWith(">"))
            {
                Console.WriteLine("TAG: " + part);
            }
            else if (!string.IsNullOrWhiteSpace(part))
            {
                Console.WriteLine("TXT: " + part);
            }
        }

        // Test entity decoding/encoding
        string complex = "r&amp;k"; // 'r' and 'k' in Nudi might be characters
        string decoded = WebUtility.HtmlDecode(complex);
        Console.WriteLine("Decoded: " + decoded);
        // Simulate conversion r->R, k->K
        string converted = decoded.ToUpper();
        string encoded = WebUtility.HtmlEncode(converted);
        Console.WriteLine("Encoded: " + encoded);
    }
}

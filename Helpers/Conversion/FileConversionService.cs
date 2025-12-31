using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class FileConversionService
    {
        public static string AsciiToUnicodeConverter(string input)
            => Convert(input, Direction.AsciiToUnicode);

        public static string UnicodeToAsciiConverter(string input)
            => Convert(input, Direction.UnicodeToAscii);

        private enum Direction { AsciiToUnicode, UnicodeToAscii }

        private static readonly Lazy<FileConversionUtilities.ConversionConfig> A2U =
            new(() => LoadConfig(Direction.AsciiToUnicode));

        private static readonly Lazy<FileConversionUtilities.ConversionConfig> U2A =
            new(() => LoadConfig(Direction.UnicodeToAscii));

        private static FileConversionUtilities.ConversionConfig GetCfg(Direction dir)
            => dir == Direction.AsciiToUnicode ? A2U.Value : U2A.Value;

        private static string Convert(string? input, Direction dir)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return input ?? string.Empty;

                var cfg = GetCfg(dir);

                return dir == Direction.AsciiToUnicode
                    ? FileConversionUtilities.ConvertAsciiToUnicode(input, cfg)
                    : FileConversionUtilities.ConvertUnicodeToAscii(input, cfg);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"[{dir}] Failed");
                return input ?? string.Empty;
            }
        }

        private static FileConversionUtilities.ConversionConfig LoadConfig(Direction dir)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = dir == Direction.AsciiToUnicode
                ? "AsciiToUnicodeMapping.json"
                : "UnicodeToAsciiMapping.json";

            string jsonPath = Path.Combine(baseDir, "Resources", fileName);

            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"{fileName} not found.", jsonPath);

            var opts = CreateJsonOptions();

            string json = File.ReadAllText(jsonPath, Encoding.UTF8);

            var root = JsonSerializer.Deserialize<FileConversionUtilities.ConversionJson>(json, opts)
                       ?? throw new InvalidOperationException($"Failed to deserialize {fileName}.");

            return FileConversionUtilities.ConversionConfig.From(root, dir == Direction.AsciiToUnicode);
        }

        private static JsonSerializerOptions CreateJsonOptions()
            => new()
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
    }
}

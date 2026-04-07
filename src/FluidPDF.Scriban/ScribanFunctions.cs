using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace FluidPDF.Scriban
{
    internal sealed class ScribanFunctions : ScriptObject
    {
        private const string Now = "now";
        private const string Today = "today";
        private const string MaxDate = "maxdate";
        private const string MinDate = "mindate";

        private static readonly Random _random = new();

        private static readonly Dictionary<string, Encoding> _encodingMap =
            new Dictionary<string, Encoding>(StringComparer.InvariantCultureIgnoreCase)
            {
                [nameof(Encoding.ASCII)] = Encoding.ASCII,
                [nameof(Encoding.BigEndianUnicode)] = Encoding.BigEndianUnicode,
                [nameof(Encoding.Unicode)] = Encoding.Unicode,
                [nameof(Encoding.UTF32)] = Encoding.UTF32,
                [nameof(Encoding.UTF8)] = Encoding.UTF8
            };

        // ── Filters ───────────────────────────────────────────────────────────────

        public static object? ToNumber(object? input)
        {
            if (input == null)
            {
                return null;
            }

            string str = input.ToString()!;

            if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
            {
                return d;
            }

            if (int.TryParse(str, out int i))
            {
                return i;
            }

            return null;
        }

        public static object? ToDateTime(object? input, string? format = null)
        {
            if (input == null)
            {
                return null;
            }

            if (input is DateTime dt)
            {
                return string.IsNullOrEmpty(format) ? (object)dt : dt.ToString(format, CultureInfo.InvariantCulture);
            }

            if (input is DateTimeOffset dto)
            {
                return string.IsNullOrEmpty(format) ? (object)dto.DateTime : dto.DateTime.ToString(format, CultureInfo.InvariantCulture);
            }

            string str = input.ToString()!.ToLowerInvariant();

            switch (str)
            {
                case Now: return string.IsNullOrEmpty(format) ? (object)DateTime.Now : DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
                case Today: return string.IsNullOrEmpty(format) ? (object)DateTime.Today : DateTime.Today.ToString(format, CultureInfo.InvariantCulture);
                case MaxDate: return string.IsNullOrEmpty(format) ? (object)DateTime.MaxValue : DateTime.MaxValue.ToString(format, CultureInfo.InvariantCulture);
                case MinDate: return string.IsNullOrEmpty(format) ? (object)DateTime.MinValue : DateTime.MinValue.ToString(format, CultureInfo.InvariantCulture);
            }

            DateTimeOffset parsed = default;
            bool hasParsed = false;

            if (!string.IsNullOrEmpty(format))
            {
                if (DateTimeOffset.TryParseExact(str, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dtExact))
                {
                    parsed = dtExact;
                    hasParsed = true;
                }
            }

            if (!hasParsed && DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dtParsed))
            {
                parsed = dtParsed;
                hasParsed = true;
            }

            if (!hasParsed)
            {
                return null;
            }

            return string.IsNullOrEmpty(format)
                ? (object)parsed.DateTime
                : parsed.DateTime.ToString(format, CultureInfo.InvariantCulture);
        }

        public static object? ToString(object? input, string? format = null, int len = 0)
        {
            if (input == null)
            {
                return null;
            }

            string result;

            if (format != null && input is IFormattable formattable)
            {
                result = formattable.ToString(format, CultureInfo.CurrentCulture);
            }
            else
            {
                object v = input;

                if (input is decimal d && (d % 1) == 0)
                {
                    v = Convert.ToInt64(input);
                }
                else if (input is double dbl && (dbl % 1) == 0)
                {
                    v = Convert.ToInt64(input);
                }
                else if (input is float f && (f % 1) == 0)
                {
                    v = Convert.ToInt64(input);
                }

                result = v.ToString()!;
            }

            if (len > 0)
            {
                result = result.PadLeft(len);
            }

            return result;
        }

        public static object? FileReadAllText(object? input, string? encode = null)
        {
            if (input == null)
            {
                return null;
            }

            string path = input.ToString()!;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return null;
            }
        }

        public static object? FileReadAllLines(object? input, string? source = null, string? removeEmpty = null)
        {
            if (input == null)
            {
                return new ScriptArray();
            }

            bool removeEmptyLines = !string.Equals(removeEmpty, "none", StringComparison.OrdinalIgnoreCase);

            if (string.Equals(source, "file", StringComparison.OrdinalIgnoreCase))
            {
                string path = input.ToString()!;

                if (!File.Exists(path))
                {
                    return new ScriptArray();
                }

                string[] fileLines = File.ReadAllLines(path);
                ScriptArray result = [];

                for (int i = 0; i < fileLines.Length; i++)
                {
                    result.Add(CreateLineData(i, fileLines[i]));
                }

                return result;
            }
            else
            {
                string[] splitLines = input.ToString()!.Split(
                    ['\n'],
                    removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

                ScriptArray result = [];

                for (int i = 0; i < splitLines.Length; i++)
                {
                    result.Add(CreateLineData(i, splitLines[i].TrimEnd('\r')));
                }

                return result;
            }
        }

        public static object? ExtractFileName(object? input)
        {
            if (input == null)
            {
                return null;
            }

            return Path.GetFileName(input.ToString()!);
        }

        public static object? ExtractDirectoryName(object? input)
        {
            if (input == null)
            {
                return null;
            }

            return Path.GetDirectoryName(input.ToString()!) ?? string.Empty;
        }

        public static object StartsWith(object? input, string? value)
        {
            if (input == null || value == null)
            {
                return false;
            }

            return input.ToString()!.StartsWith(value, StringComparison.Ordinal);
        }

        public static object EndsWith(object? input, string? value)
        {
            if (input == null || value == null)
            {
                return false;
            }

            return input.ToString()!.EndsWith(value, StringComparison.Ordinal);
        }

        public static object Contains(object? input, string? value)
        {
            if (input == null || value == null)
            {
                return false;
            }

            return input.ToString()!.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        public static object? ToBase64(object? input, string encoding = "UTF8")
        {
            if (input == null)
            {
                return null;
            }

            try
            {
                Encoding enc = GetEncoding(encoding);
                byte[] bytes = enc.GetBytes(input.ToString()!);
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                return $"{ex.Message} - {ex.StackTrace}";
            }
        }

        public static object? FromBase64(object? input, string encoding = "UTF8")
        {
            if (input == null)
            {
                return null;
            }

            try
            {
                Encoding enc = GetEncoding(encoding);
                byte[] bytes = Convert.FromBase64String(input.ToString()!);
                return enc.GetString(bytes);
            }
            catch (Exception ex)
            {
                return $"{ex.Message} - {ex.StackTrace}";
            }
        }

        // ── Functions (equivalent to Fluid tags) ──────────────────────────────────

        public static object FloatRandom() => _random.NextDouble();

        public static object Guid(string type = "new") =>
            type?.ToLowerInvariant() switch
            {
                "empty" => System.Guid.Empty.ToString(),
                _ => System.Guid.NewGuid().ToString()
            };

        public static object IntRandom(int min = 0, int max = int.MaxValue) =>
            _random.Next(min, max);

        public static object StringEmpty() => string.Empty;

        public static object Backslash() => "\\";

        public static object Slash() => "/";

        public static object Pipe() => "|";

        public static object DoubleQuote() => "\"";

        public static object SingleQuote() => "'";

        public static object PathSeparator() => Path.DirectorySeparatorChar.ToString();

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static ScriptObject CreateLineData(int lineNumber, string lineContent)
        {
            ScriptObject obj = [];
            obj.Add("LineNumber", lineNumber);
            obj.Add("LineContent", lineContent);
            return obj;
        }

        private static Encoding GetEncoding(string name)
        {
            if (_encodingMap.TryGetValue(name, out Encoding? enc))
            {
                return enc;
            }

            return Encoding.GetEncoding(name);
        }
    }
}

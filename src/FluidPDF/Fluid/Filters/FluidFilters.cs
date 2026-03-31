using Fluid;
using Fluid.Values;
using FluidPDF.Support.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluidPDF.Fluid.Filters
{
    internal static class FluidFilters
    {
        private const string Now = "now";
        private const string Today = "today";
        private const string MaxDate = "maxdate";
        private const string MinDate = "mindate";

        private static readonly Dictionary<string, Encoding> _encodingMap =
            new Dictionary<string, Encoding>(StringComparer.InvariantCultureIgnoreCase)
            {
                [nameof(Encoding.ASCII)] = Encoding.ASCII,
                [nameof(Encoding.BigEndianUnicode)] = Encoding.BigEndianUnicode,
                [nameof(Encoding.Unicode)] = Encoding.Unicode,
                [nameof(Encoding.UTF32)] = Encoding.UTF32,
                [nameof(Encoding.UTF8)] = Encoding.UTF8
            };

        internal static void Register(TemplateOptions options)
        {
            options.Filters.AddFilter("to_number", ToNumber);
            options.Filters.AddFilter("to_date_time", ToDateTime);
            options.Filters.AddFilter("to_string", ToStringFilter);
            options.Filters.AddFilter("file_read_all_text", FileReadAllText);
            options.Filters.AddFilter("file_read_all_lines", FileReadAllLinesAsync);
            options.Filters.AddFilter("extract_file_name", ExtractFileName);
            options.Filters.AddFilter("extract_directory_name", ExtractDirectoryName);
            options.Filters.AddFilter("starts_with", StartsWith);
            options.Filters.AddFilter("ends_with", EndsWith);
            options.Filters.AddFilter("contains", Contains);
            options.Filters.AddFilter("to_base64", ToBase64);
            options.Filters.AddFilter("from_base64", FromBase64);

            options.MemberAccessStrategy.Register<FileLineData>();
        }

        public static ValueTask<FluidValue> ToNumber(FluidValue input, FilterArguments arguments, TemplateContext context) =>
            NumberValue.Create(input.ToNumberValue());

        public static ValueTask<FluidValue> ToDateTime(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            string? outputFormat = null;
            if (arguments.Count > 0)
            {
                outputFormat = arguments.At(0).ToStringValue();
            }

            if (!TryGetDateTimeInput(input, context, out DateTimeOffset value))
            {
                return NilValue.Instance;
            }

            if (string.IsNullOrEmpty(outputFormat))
            {
                return new DateTimeValue(value);
            }

            return new ValueTask<FluidValue>(new StringValue(value.ToString(outputFormat, context.CultureInfo)));
        }

        public static ValueTask<FluidValue> ToStringFilter(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.IsNil())
            {
                return new ValueTask<FluidValue>(input);
            }

            const string formatParam = "format";
            const string lenParam = "len";

            string? format = null;
            int? len = null;

            if (arguments.HasNamed(formatParam))
            {
                format = arguments[formatParam].ToStringValue();
            }

            if (arguments.HasNamed(lenParam))
            {
                len = Convert.ToInt32(arguments[lenParam].ToNumberValue());
            }

            string finalFormat = $"{{0{(len.HasValue ? $",{len.Value}" : string.Empty)}{(string.IsNullOrWhiteSpace(format) ? string.Empty : ":" + format)}}}";

            try
            {
                object v = input.ToObjectValue();
                if (input.Type == FluidValues.Number)
                {
                    decimal d = (decimal)v;
                    if ((d % 1) == 0)
                    {
                        v = Convert.ToInt64(v);
                    }
                }

                string formattedString = string.Format(context.CultureInfo, finalFormat, v);
                return new ValueTask<FluidValue>(new StringValue(formattedString));
            }
            catch (Exception)
            {
                return new ValueTask<FluidValue>(new StringValue(input.ToStringValue()));
            }
        }

        public static async ValueTask<FluidValue> FileReadAllText(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String)
            {
                return NilValue.Instance;
            }

            string inputFileName = input.ToStringValue();

            if (!File.Exists(inputFileName))
            {
                return NilValue.Instance;
            }

            bool encode = false;
            if (arguments.Count > 0)
            {
                if (string.Equals(arguments.At(0).ToStringValue(), "encode", StringComparison.CurrentCultureIgnoreCase))
                {
                    encode = true;
                }
            }

            string fileContent = await FileHelper.ReadAllTextAsync(inputFileName).ConfigureAwait(false);
            return new StringValue(fileContent, encode);
        }

        public static async ValueTask<FluidValue> FileReadAllLinesAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String)
            {
                return ArrayValue.Empty;
            }

            string source = input.ToStringValue();

            bool isSourceFile = false;
            bool removeEmptyLines = true;

            if (arguments.Count > 0)
            {
                if (string.Equals(arguments.At(0).ToStringValue(), "file", StringComparison.CurrentCultureIgnoreCase))
                {
                    isSourceFile = true;
                }

                FluidValue arg2 = arguments.At(1);
                if (!arg2.IsNil())
                {
                    removeEmptyLines = string.Equals(arg2.ToStringValue(), "none", StringComparison.CurrentCultureIgnoreCase);
                }
            }

            if (isSourceFile && !File.Exists(source))
            {
                return ArrayValue.Empty;
            }

            FileLineData[] retData;

            if (isSourceFile)
            {
                string[] lines = await FileHelper.ReadAllLinesAsync(source).ConfigureAwait(false);
                retData = lines
                    .Select((s, i) => new FileLineData(i, s))
                    .ToArray();
            }
            else
            {
                retData = source
                    .Split(['\n'], removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
                    .Select((s, i) => new FileLineData(i, s.TrimEnd('\r')))
                    .ToArray();
            }

            return ArrayValue.Create(retData, context.Options);
        }

        public static ValueTask<FluidValue> ExtractFileName(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String)
            {
                return new ValueTask<FluidValue>(input);
            }

            string retValue = Path.GetFileName(input.ToStringValue());
            return new ValueTask<FluidValue>(new StringValue(retValue, false));
        }

        public static ValueTask<FluidValue> ExtractDirectoryName(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String)
            {
                return new ValueTask<FluidValue>(input);
            }

            string retValue = Path.GetDirectoryName(input.ToStringValue()) ?? string.Empty;
            return new ValueTask<FluidValue>(new StringValue(retValue, false));
        }

        public static ValueTask<FluidValue> StartsWith(FluidValue input, FilterArguments arguments, TemplateContext context) =>
            StringBoolFx(input, arguments, context, (s, v) => s.StartsWith(v, StringComparison.Ordinal));

        public static ValueTask<FluidValue> EndsWith(FluidValue input, FilterArguments arguments, TemplateContext context) =>
            StringBoolFx(input, arguments, context, (s, v) => s.EndsWith(v, StringComparison.Ordinal));

        public static ValueTask<FluidValue> Contains(FluidValue input, FilterArguments arguments, TemplateContext context) =>
            StringBoolFx(input, arguments, context, (s, v) => s.Contains(v, StringComparison.Ordinal));

        public static ValueTask<FluidValue> ToBase64(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String || input.IsNil())
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            string stringValue = input.ToStringValue();

            try
            {
                Encoding encoding = GetEncoding(arguments);
                string encodedValue = Convert.ToBase64String(encoding.GetBytes(stringValue));
                return new ValueTask<FluidValue>(new StringValue(encodedValue));
            }
            catch (Exception ex)
            {
                return new ValueTask<FluidValue>(new StringValue($"{ex.Message} - {ex.StackTrace}"));
            }
        }

        public static ValueTask<FluidValue> FromBase64(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.Type != FluidValues.String || input.IsNil())
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            string stringValue = input.ToStringValue();

            try
            {
                Encoding encoding = GetEncoding(arguments);
                string decodedValue = encoding.GetString(Convert.FromBase64String(stringValue));
                return new ValueTask<FluidValue>(new StringValue(decodedValue));
            }
            catch (Exception ex)
            {
                return new ValueTask<FluidValue>(new StringValue($"{ex.Message} - {ex.StackTrace}"));
            }
        }

        private static bool TryGetDateTimeInput(FluidValue input, TemplateContext context, out DateTimeOffset result)
        {
            result = context.Now();

            if (input.Type == FluidValues.String)
            {
                string stringValue = input.ToStringValue();

                if (stringValue == Now)
                {
                    return true;
                }
                else if (stringValue == Today)
                {
                    result = result.Date;
                    return true;
                }
                else if (stringValue == MaxDate)
                {
                    result = DateTimeOffset.MaxValue;
                    return true;
                }
                else if (stringValue == MinDate)
                {
                    result = DateTimeOffset.MinValue;
                    return true;
                }
                else
                {
                    return DateTimeOffset.TryParse(stringValue, context.CultureInfo, DateTimeStyles.AssumeUniversal, out result);
                }
            }
            else if (input.Type == FluidValues.DateTime)
            {
                result = (DateTimeOffset)input.ToObjectValue();
            }
            else
            {
                object objValue = input.ToObjectValue();

                if (objValue is DateTime dateTime)
                {
                    result = dateTime;
                }
                else if (objValue is DateTimeOffset dateTimeOffset)
                {
                    result = dateTimeOffset;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static ValueTask<FluidValue> StringBoolFx(FluidValue input, FilterArguments arguments, TemplateContext context, Func<string, string, bool> fx)
        {
            if (input.Type != FluidValues.String)
            {
                return new ValueTask<FluidValue>(BooleanValue.False);
            }

            string srcStr = input.ToStringValue();
            string s = arguments.At(0).ToStringValue();
            return new ValueTask<FluidValue>(BooleanValue.Create(fx(srcStr, s)));
        }

        private static Encoding GetEncoding(FilterArguments arguments)
        {
            const string encodingParam = "encoding";
            string encodingName = "UTF8";

            if (arguments.HasNamed(encodingParam))
            {
                encodingName = arguments[encodingParam].ToStringValue();
            }

            if (!_encodingMap.TryGetValue(encodingName, out Encoding? encoding))
            {
                encoding = Encoding.GetEncoding(encodingName);
            }

            return encoding;
        }
    }
}

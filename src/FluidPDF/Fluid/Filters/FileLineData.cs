namespace FluidPDF.Fluid.Filters
{
    /// <summary>
    /// Represents a single line from a file or string, carrying both its zero-based
    /// line number and its text content. Used by the <c>file_read_all_lines</c> filter.
    /// </summary>
    public sealed class FileLineData
    {
        public int LineNumber { get; }
        public string LineContent { get; }

        public FileLineData(int lineNumber, string lineContent)
        {
            LineNumber = lineNumber;
            LineContent = lineContent;
        }
    }
}

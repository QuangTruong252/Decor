namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// Request configuration for Excel export operations
    /// </summary>
    public class ExcelExportRequestDTO
    {
        /// <summary>
        /// Name of the worksheet
        /// </summary>
        public string WorksheetName { get; set; } = "Export";

        /// <summary>
        /// Custom column headers (if different from property names)
        /// </summary>
        public Dictionary<string, string> ColumnHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Columns to include in export (if null, all columns are included)
        /// </summary>
        public List<string>? ColumnsToInclude { get; set; }

        /// <summary>
        /// Columns to exclude from export
        /// </summary>
        public List<string> ColumnsToExclude { get; set; } = new List<string>();

        /// <summary>
        /// Custom formatting for specific columns
        /// </summary>
        public Dictionary<string, ExcelColumnFormatDTO> ColumnFormats { get; set; } = new Dictionary<string, ExcelColumnFormatDTO>();

        /// <summary>
        /// Whether to include filters in the Excel file
        /// </summary>
        public bool IncludeFilters { get; set; } = true;

        /// <summary>
        /// Whether to freeze the header row
        /// </summary>
        public bool FreezeHeaderRow { get; set; } = true;

        /// <summary>
        /// Whether to auto-fit column widths
        /// </summary>
        public bool AutoFitColumns { get; set; } = true;

        /// <summary>
        /// Maximum number of rows to export (0 = no limit)
        /// </summary>
        public int MaxRows { get; set; } = 0;

        /// <summary>
        /// Custom title for the export (appears above headers)
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Additional metadata to include in the export
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Date format to use for DateTime columns
        /// </summary>
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        /// <summary>
        /// Time format to use for DateTime columns with time
        /// </summary>
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Number format for decimal columns
        /// </summary>
        public string DecimalFormat { get; set; } = "#,##0.00";

        /// <summary>
        /// Whether to include row numbers
        /// </summary>
        public bool IncludeRowNumbers { get; set; } = false;

        /// <summary>
        /// Custom styling options
        /// </summary>
        public ExcelStyleOptionsDTO StyleOptions { get; set; } = new ExcelStyleOptionsDTO();
    }

    /// <summary>
    /// Column formatting options for Excel export
    /// </summary>
    public class ExcelColumnFormatDTO
    {
        /// <summary>
        /// Number format string
        /// </summary>
        public string? NumberFormat { get; set; }

        /// <summary>
        /// Column width (0 = auto-fit)
        /// </summary>
        public double Width { get; set; } = 0;

        /// <summary>
        /// Text alignment
        /// </summary>
        public ExcelAlignment Alignment { get; set; } = ExcelAlignment.Left;

        /// <summary>
        /// Whether to wrap text
        /// </summary>
        public bool WrapText { get; set; } = false;

        /// <summary>
        /// Background color (hex format)
        /// </summary>
        public string? BackgroundColor { get; set; }

        /// <summary>
        /// Font color (hex format)
        /// </summary>
        public string? FontColor { get; set; }

        /// <summary>
        /// Whether text should be bold
        /// </summary>
        public bool Bold { get; set; } = false;

        /// <summary>
        /// Whether text should be italic
        /// </summary>
        public bool Italic { get; set; } = false;
    }

    /// <summary>
    /// Styling options for Excel export
    /// </summary>
    public class ExcelStyleOptionsDTO
    {
        /// <summary>
        /// Header row styling
        /// </summary>
        public ExcelColumnFormatDTO HeaderStyle { get; set; } = new ExcelColumnFormatDTO
        {
            Bold = true,
            BackgroundColor = "#4472C4",
            FontColor = "#FFFFFF",
            Alignment = ExcelAlignment.Center
        };

        /// <summary>
        /// Alternating row colors
        /// </summary>
        public bool UseAlternatingRowColors { get; set; } = true;

        /// <summary>
        /// Primary row color
        /// </summary>
        public string PrimaryRowColor { get; set; } = "#FFFFFF";

        /// <summary>
        /// Alternate row color
        /// </summary>
        public string AlternateRowColor { get; set; } = "#F2F2F2";

        /// <summary>
        /// Border style for cells
        /// </summary>
        public ExcelCellBorderStyle BorderStyle { get; set; } = ExcelCellBorderStyle.Thin;

        /// <summary>
        /// Border color
        /// </summary>
        public string BorderColor { get; set; } = "#000000";

        /// <summary>
        /// Font name
        /// </summary>
        public string FontName { get; set; } = "Calibri";

        /// <summary>
        /// Font size
        /// </summary>
        public int FontSize { get; set; } = 11;
    }

    /// <summary>
    /// Text alignment options
    /// </summary>
    public enum ExcelAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    /// <summary>
    /// Border style options
    /// </summary>
    public enum ExcelCellBorderStyle
    {
        None,
        Thin,
        Medium,
        Thick
    }
}

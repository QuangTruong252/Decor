namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// Result of Excel import operation
    /// </summary>
    /// <typeparam name="T">Type of imported data</typeparam>
    public class ExcelImportResultDTO<T> where T : class
    {
        /// <summary>
        /// Successfully imported data
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Validation errors encountered during import
        /// </summary>
        public List<ExcelValidationErrorDTO> Errors { get; set; } = new List<ExcelValidationErrorDTO>();

        /// <summary>
        /// Total number of rows processed
        /// </summary>
        public int TotalRows { get; set; }

        /// <summary>
        /// Number of successfully imported rows
        /// </summary>
        public int SuccessfulRows { get; set; }

        /// <summary>
        /// Number of rows with errors
        /// </summary>
        public int ErrorRows { get; set; }

        /// <summary>
        /// Whether the import was successful (no errors)
        /// </summary>
        public bool IsSuccess => Errors.Count == 0;

        /// <summary>
        /// Summary of the import operation
        /// </summary>
        public string Summary => $"Processed {TotalRows} rows. Success: {SuccessfulRows}, Errors: {ErrorRows}";

        /// <summary>
        /// Time taken for the import operation
        /// </summary>
        public TimeSpan ProcessingTime { get; set; }

        /// <summary>
        /// Additional metadata about the import
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Chunk of data for large file processing
    /// </summary>
    /// <typeparam name="T">Type of data in chunk</typeparam>
    public class ExcelImportChunkDTO<T> where T : class
    {
        /// <summary>
        /// Data in this chunk
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Errors in this chunk
        /// </summary>
        public List<ExcelValidationErrorDTO> Errors { get; set; } = new List<ExcelValidationErrorDTO>();

        /// <summary>
        /// Chunk number (1-based)
        /// </summary>
        public int ChunkNumber { get; set; }

        /// <summary>
        /// Starting row number for this chunk
        /// </summary>
        public int StartRow { get; set; }

        /// <summary>
        /// Ending row number for this chunk
        /// </summary>
        public int EndRow { get; set; }

        /// <summary>
        /// Whether this is the last chunk
        /// </summary>
        public bool IsLastChunk { get; set; }
    }

    /// <summary>
    /// Result of Excel file validation
    /// </summary>
    public class ExcelValidationResultDTO
    {
        /// <summary>
        /// Whether the file is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Validation warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// File information
        /// </summary>
        public ExcelFileInfoDTO FileInfo { get; set; } = new ExcelFileInfoDTO();

        /// <summary>
        /// Detected columns in the file
        /// </summary>
        public List<string> DetectedColumns { get; set; } = new List<string>();

        /// <summary>
        /// Missing required columns
        /// </summary>
        public List<string> MissingColumns { get; set; } = new List<string>();

        /// <summary>
        /// Extra columns not in mapping
        /// </summary>
        public List<string> ExtraColumns { get; set; } = new List<string>();
    }

    /// <summary>
    /// Excel file information
    /// </summary>
    public class ExcelFileInfoDTO
    {
        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Number of worksheets
        /// </summary>
        public int WorksheetCount { get; set; }

        /// <summary>
        /// Number of rows in the main worksheet
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Number of columns in the main worksheet
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// Worksheet names
        /// </summary>
        public List<string> WorksheetNames { get; set; } = new List<string>();

        /// <summary>
        /// File format (xlsx, xls, etc.)
        /// </summary>
        public string FileFormat { get; set; } = string.Empty;
    }
}

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// Represents a validation error during Excel import
    /// </summary>
    public class ExcelValidationErrorDTO
    {
        /// <summary>
        /// Row number where the error occurred (1-based)
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Column name where the error occurred
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// The invalid value that caused the error
        /// </summary>
        public string? InvalidValue { get; set; }

        /// <summary>
        /// Error message describing what went wrong
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Error code for programmatic handling
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Severity of the error
        /// </summary>
        public ExcelErrorSeverity Severity { get; set; } = ExcelErrorSeverity.Error;

        /// <summary>
        /// Property name in the DTO that failed validation
        /// </summary>
        public string? PropertyName { get; set; }

        /// <summary>
        /// Suggested fix for the error
        /// </summary>
        public string? SuggestedFix { get; set; }

        /// <summary>
        /// Additional context about the error
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Creates a new validation error
        /// </summary>
        public ExcelValidationErrorDTO() { }

        /// <summary>
        /// Creates a new validation error with basic information
        /// </summary>
        /// <param name="rowNumber">Row number (1-based)</param>
        /// <param name="columnName">Column name</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="invalidValue">Invalid value</param>
        public ExcelValidationErrorDTO(int rowNumber, string columnName, string errorMessage, string? invalidValue = null)
        {
            RowNumber = rowNumber;
            ColumnName = columnName;
            ErrorMessage = errorMessage;
            InvalidValue = invalidValue;
        }

        /// <summary>
        /// Creates a new validation error with full information
        /// </summary>
        /// <param name="rowNumber">Row number (1-based)</param>
        /// <param name="columnName">Column name</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="severity">Error severity</param>
        /// <param name="invalidValue">Invalid value</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="suggestedFix">Suggested fix</param>
        public ExcelValidationErrorDTO(
            int rowNumber,
            string columnName,
            string errorMessage,
            string errorCode,
            ExcelErrorSeverity severity = ExcelErrorSeverity.Error,
            string? invalidValue = null,
            string? propertyName = null,
            string? suggestedFix = null)
        {
            RowNumber = rowNumber;
            ColumnName = columnName;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            Severity = severity;
            InvalidValue = invalidValue;
            PropertyName = propertyName;
            SuggestedFix = suggestedFix;
        }

        /// <summary>
        /// Returns a formatted string representation of the error
        /// </summary>
        public override string ToString()
        {
            var severityText = Severity switch
            {
                ExcelErrorSeverity.Warning => "WARNING",
                ExcelErrorSeverity.Error => "ERROR",
                ExcelErrorSeverity.Critical => "CRITICAL",
                _ => "ERROR"
            };

            var valueText = !string.IsNullOrEmpty(InvalidValue) ? $" (Value: '{InvalidValue}')" : "";
            return $"Row {RowNumber}, Column '{ColumnName}': {severityText} - {ErrorMessage}{valueText}";
        }
    }

    /// <summary>
    /// Severity levels for Excel validation errors
    /// </summary>
    public enum ExcelErrorSeverity
    {
        /// <summary>
        /// Warning - data can be imported but may need attention
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error - data cannot be imported as-is
        /// </summary>
        Error = 2,

        /// <summary>
        /// Critical error - stops the entire import process
        /// </summary>
        Critical = 3
    }

    /// <summary>
    /// Common error codes for Excel validation
    /// </summary>
    public static class ExcelErrorCodes
    {
        public const string REQUIRED_FIELD_MISSING = "REQUIRED_FIELD_MISSING";
        public const string INVALID_DATA_TYPE = "INVALID_DATA_TYPE";
        public const string INVALID_FORMAT = "INVALID_FORMAT";
        public const string INVALID_VALUE = "INVALID_VALUE";
        public const string VALUE_OUT_OF_RANGE = "VALUE_OUT_OF_RANGE";
        public const string DUPLICATE_VALUE = "DUPLICATE_VALUE";
        public const string FOREIGN_KEY_NOT_FOUND = "FOREIGN_KEY_NOT_FOUND";
        public const string BUSINESS_RULE_VIOLATION = "BUSINESS_RULE_VIOLATION";
        public const string STRING_TOO_LONG = "STRING_TOO_LONG";
        public const string INVALID_EMAIL = "INVALID_EMAIL";
        public const string INVALID_PHONE = "INVALID_PHONE";
        public const string INVALID_URL = "INVALID_URL";
        public const string INVALID_DATE = "INVALID_DATE";
        public const string INVALID_DECIMAL = "INVALID_DECIMAL";
        public const string INVALID_INTEGER = "INVALID_INTEGER";
        public const string INVALID_BOOLEAN = "INVALID_BOOLEAN";
        public const string COLUMN_NOT_FOUND = "COLUMN_NOT_FOUND";
        public const string FILE_FORMAT_ERROR = "FILE_FORMAT_ERROR";
        public const string FILE_TOO_LARGE = "FILE_TOO_LARGE";
        public const string WORKSHEET_NOT_FOUND = "WORKSHEET_NOT_FOUND";
    }
}

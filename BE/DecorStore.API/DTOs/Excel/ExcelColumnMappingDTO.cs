using System.ComponentModel.DataAnnotations;

namespace DecorStore.API.DTOs.Excel
{
    /// <summary>
    /// Configuration for mapping Excel columns to DTO properties
    /// </summary>
    public class ExcelColumnMappingDTO
    {
        /// <summary>
        /// Property name in the DTO
        /// </summary>
        [Required]
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Excel column header name
        /// </summary>
        [Required]
        public string ColumnHeader { get; set; } = string.Empty;

        /// <summary>
        /// Alternative column names that can be matched
        /// </summary>
        public List<string> AlternativeHeaders { get; set; } = new List<string>();

        /// <summary>
        /// Whether this column is required
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Data type of the property
        /// </summary>
        public ExcelDataType DataType { get; set; } = ExcelDataType.String;

        /// <summary>
        /// Default value to use if cell is empty
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Custom validation rules for this column
        /// </summary>
        public List<ExcelColumnValidationRule> ValidationRules { get; set; } = new List<ExcelColumnValidationRule>();

        /// <summary>
        /// Custom transformation function name
        /// </summary>
        public string? TransformationFunction { get; set; }

        /// <summary>
        /// Format string for parsing/formatting values
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Whether to trim whitespace from string values
        /// </summary>
        public bool TrimWhitespace { get; set; } = true;

        /// <summary>
        /// Whether empty strings should be treated as null
        /// </summary>
        public bool TreatEmptyAsNull { get; set; } = true;

        /// <summary>
        /// Maximum length for string values
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Minimum value for numeric types
        /// </summary>
        public object? MinValue { get; set; }

        /// <summary>
        /// Maximum value for numeric types
        /// </summary>
        public object? MaxValue { get; set; }

        /// <summary>
        /// Regular expression pattern for validation
        /// </summary>
        public string? RegexPattern { get; set; }

        /// <summary>
        /// Error message to show when validation fails
        /// </summary>
        public string? ValidationErrorMessage { get; set; }

        /// <summary>
        /// Description of the column for documentation
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Example values for this column
        /// </summary>
        public List<string> ExampleValues { get; set; } = new List<string>();

        /// <summary>
        /// Whether this column should be included in exports
        /// </summary>
        public bool IncludeInExport { get; set; } = true;

        /// <summary>
        /// Whether this column should be included in imports
        /// </summary>
        public bool IncludeInImport { get; set; } = true;

        /// <summary>
        /// Order of this column in exports (lower numbers first)
        /// </summary>
        public int ExportOrder { get; set; } = 0;
    }

    /// <summary>
    /// Data types supported in Excel import/export
    /// </summary>
    public enum ExcelDataType
    {
        String,
        Integer,
        Decimal,
        Boolean,
        DateTime,
        Date,
        Time,
        Email,
        Phone,
        Url,
        Guid,
        Enum
    }

    /// <summary>
    /// Validation rule for Excel columns
    /// </summary>
    public class ExcelColumnValidationRule
    {
        /// <summary>
        /// Type of validation rule
        /// </summary>
        public ExcelValidationType ValidationType { get; set; }

        /// <summary>
        /// Value to validate against (for range, length, etc.)
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// Second value for range validations
        /// </summary>
        public object? SecondValue { get; set; }

        /// <summary>
        /// Error message when validation fails
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a warning or error
        /// </summary>
        public ExcelErrorSeverity Severity { get; set; } = ExcelErrorSeverity.Error;
    }

    /// <summary>
    /// Types of validation that can be applied to Excel columns
    /// </summary>
    public enum ExcelValidationType
    {
        Required,
        MinLength,
        MaxLength,
        MinValue,
        MaxValue,
        Range,
        RegexPattern,
        Email,
        Phone,
        Url,
        Custom,
        UniqueValue,
        ForeignKey
    }

    /// <summary>
    /// Helper class for creating common column mappings
    /// </summary>
    public static class ExcelColumnMappingHelper
    {
        /// <summary>
        /// Creates a basic string column mapping
        /// </summary>
        public static ExcelColumnMappingDTO CreateStringColumn(string propertyName, string columnHeader, bool isRequired = false, int? maxLength = null)
        {
            return new ExcelColumnMappingDTO
            {
                PropertyName = propertyName,
                ColumnHeader = columnHeader,
                DataType = ExcelDataType.String,
                IsRequired = isRequired,
                MaxLength = maxLength
            };
        }

        /// <summary>
        /// Creates a numeric column mapping
        /// </summary>
        public static ExcelColumnMappingDTO CreateNumericColumn(string propertyName, string columnHeader, bool isRequired = false, object? minValue = null, object? maxValue = null)
        {
            return new ExcelColumnMappingDTO
            {
                PropertyName = propertyName,
                ColumnHeader = columnHeader,
                DataType = ExcelDataType.Decimal,
                IsRequired = isRequired,
                MinValue = minValue,
                MaxValue = maxValue
            };
        }

        /// <summary>
        /// Creates a date column mapping
        /// </summary>
        public static ExcelColumnMappingDTO CreateDateColumn(string propertyName, string columnHeader, bool isRequired = false, string? format = null)
        {
            return new ExcelColumnMappingDTO
            {
                PropertyName = propertyName,
                ColumnHeader = columnHeader,
                DataType = ExcelDataType.DateTime,
                IsRequired = isRequired,
                Format = format ?? "yyyy-MM-dd"
            };
        }

        /// <summary>
        /// Creates an email column mapping
        /// </summary>
        public static ExcelColumnMappingDTO CreateEmailColumn(string propertyName, string columnHeader, bool isRequired = false)
        {
            return new ExcelColumnMappingDTO
            {
                PropertyName = propertyName,
                ColumnHeader = columnHeader,
                DataType = ExcelDataType.Email,
                IsRequired = isRequired,
                ValidationRules = new List<ExcelColumnValidationRule>
                {
                    new ExcelColumnValidationRule
                    {
                        ValidationType = ExcelValidationType.Email,
                        ErrorMessage = "Invalid email format"
                    }
                }
            };
        }

        /// <summary>
        /// Creates a boolean column mapping
        /// </summary>
        public static ExcelColumnMappingDTO CreateBooleanColumn(string propertyName, string columnHeader, bool isRequired = false, bool defaultValue = false)
        {
            return new ExcelColumnMappingDTO
            {
                PropertyName = propertyName,
                ColumnHeader = columnHeader,
                DataType = ExcelDataType.Boolean,
                IsRequired = isRequired,
                DefaultValue = defaultValue
            };
        }
    }
}

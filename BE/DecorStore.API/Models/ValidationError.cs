namespace DecorStore.API.Models
{
    /// <summary>
    /// Represents a validation error with detailed information
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// The name of the property that failed validation
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// The value that was attempted to be set
        /// </summary>
        public object? AttemptedValue { get; set; }

        /// <summary>
        /// The error code for this validation failure
        /// </summary>
        public string ErrorCode { get; set; } = "VALIDATION_ERROR";

        /// <summary>
        /// The severity of the validation error
        /// </summary>
        public string Severity { get; set; } = "Error";

        /// <summary>
        /// Additional metadata about the validation error
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Creates a new validation error
        /// </summary>
        public ValidationError()
        {
        }

        /// <summary>
        /// Creates a new validation error with basic information
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorCode">Error code</param>
        public ValidationError(string propertyName, string errorMessage, string errorCode = "VALIDATION_ERROR")
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a new validation error with full information
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="attemptedValue">Attempted value</param>
        /// <param name="errorCode">Error code</param>
        /// <param name="severity">Severity level</param>
        public ValidationError(string propertyName, string errorMessage, object? attemptedValue, 
            string errorCode = "VALIDATION_ERROR", string severity = "Error")
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            AttemptedValue = attemptedValue;
            ErrorCode = errorCode;
            Severity = severity;
        }
    }
}

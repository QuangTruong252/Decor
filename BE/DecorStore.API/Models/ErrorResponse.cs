using System.Text.Json.Serialization;

namespace DecorStore.API.Models
{
    /// <summary>
    /// Standardized error response model for all API errors
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique correlation ID for tracking the request
        /// </summary>
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// Error code for categorizing the error type
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error information (for debugging)
        /// </summary>
        [JsonPropertyName("details")]
        public string? Details { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Request path where the error occurred
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Validation errors grouped by field name
        /// </summary>
        [JsonPropertyName("validationErrors")]
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        /// <summary>
        /// Additional error metadata
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Error severity level
        /// </summary>
        [JsonPropertyName("severity")]
        public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;

        /// <summary>
        /// Suggested actions for resolving the error
        /// </summary>
        [JsonPropertyName("suggestedActions")]
        public string[]? SuggestedActions { get; set; }

        public ErrorResponse()
        {
        }

        public ErrorResponse(string correlationId, string errorCode, string message, string path)
        {
            CorrelationId = correlationId;
            ErrorCode = errorCode;
            Message = message;
            Path = path;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an error response for validation failures
        /// </summary>
        public static ErrorResponse ValidationError(string correlationId, string path, Dictionary<string, string[]> validationErrors)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred.",
                Path = path,
                ValidationErrors = validationErrors,
                Severity = ErrorSeverity.Warning,
                SuggestedActions = new[] { "Please correct the validation errors and try again." }
            };
        }

        /// <summary>
        /// Creates an error response for not found errors
        /// </summary>
        public static ErrorResponse NotFound(string correlationId, string path, string resource)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "NOT_FOUND",
                Message = $"The requested {resource} was not found.",
                Path = path,
                Severity = ErrorSeverity.Warning,
                SuggestedActions = new[] { "Please verify the resource identifier and try again." }
            };
        }

        /// <summary>
        /// Creates an error response for unauthorized access
        /// </summary>
        public static ErrorResponse Unauthorized(string correlationId, string path)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "UNAUTHORIZED",
                Message = "Authentication is required to access this resource.",
                Path = path,
                Severity = ErrorSeverity.Error,
                SuggestedActions = new[] { "Please provide valid authentication credentials." }
            };
        }

        /// <summary>
        /// Creates an error response for forbidden access
        /// </summary>
        public static ErrorResponse Forbidden(string correlationId, string path)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "FORBIDDEN",
                Message = "You do not have permission to access this resource.",
                Path = path,
                Severity = ErrorSeverity.Error,
                SuggestedActions = new[] { "Please contact an administrator for access." }
            };
        }

        /// <summary>
        /// Creates an error response for business rule violations
        /// </summary>
        public static ErrorResponse BusinessRuleViolation(string correlationId, string path, string rule, string details)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "BUSINESS_RULE_VIOLATION",
                Message = $"Business rule violation: {rule}",
                Path = path,
                Details = details,
                Severity = ErrorSeverity.Warning
            };
        }

        /// <summary>
        /// Creates an error response for internal server errors
        /// </summary>
        public static ErrorResponse InternalServerError(string correlationId, string path, string? details = null)
        {
            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = "INTERNAL_SERVER_ERROR",
                Message = "An unexpected error occurred while processing your request.",
                Path = path,
                Details = details,
                Severity = ErrorSeverity.Critical,
                SuggestedActions = new[] { "Please try again later or contact support if the problem persists." }
            };
        }
    }

    /// <summary>
    /// Error severity levels
    /// </summary>
    public enum ErrorSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }
}

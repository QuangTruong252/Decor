namespace DecorStore.API.Exceptions
{
    /// <summary>
    /// Exception thrown when external service calls fail
    /// </summary>
    public class ExternalServiceException : Exception
    {
        /// <summary>
        /// The name of the external service
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// The operation that was being performed
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// HTTP status code if applicable
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// Error code for categorization
        /// </summary>
        public string ErrorCode { get; set; }

        public ExternalServiceException(string serviceName, string operation) 
            : base($"External service call failed: {serviceName}.{operation}")
        {
            ServiceName = serviceName;
            Operation = operation;
            ErrorCode = "EXTERNAL_SERVICE_ERROR";
        }

        public ExternalServiceException(string serviceName, string operation, int statusCode) 
            : base($"External service call failed: {serviceName}.{operation} (Status: {statusCode})")
        {
            ServiceName = serviceName;
            Operation = operation;
            StatusCode = statusCode;
            ErrorCode = "EXTERNAL_SERVICE_ERROR";
        }

        public ExternalServiceException(string serviceName, string operation, Exception innerException) 
            : base($"External service call failed: {serviceName}.{operation}", innerException)
        {
            ServiceName = serviceName;
            Operation = operation;
            ErrorCode = "EXTERNAL_SERVICE_ERROR";
        }

        public ExternalServiceException(string serviceName, string operation, int statusCode, Exception innerException) 
            : base($"External service call failed: {serviceName}.{operation} (Status: {statusCode})", innerException)
        {
            ServiceName = serviceName;
            Operation = operation;
            StatusCode = statusCode;
            ErrorCode = "EXTERNAL_SERVICE_ERROR";
        }

        /// <summary>
        /// Creates an exception for service timeout
        /// </summary>
        public static ExternalServiceException Timeout(string serviceName, string operation, Exception innerException)
        {
            var exception = new ExternalServiceException(serviceName, operation, innerException);
            exception.ErrorCode = "EXTERNAL_SERVICE_TIMEOUT";
            return exception;
        }

        /// <summary>
        /// Creates an exception for service unavailable
        /// </summary>
        public static ExternalServiceException Unavailable(string serviceName, string operation)
        {
            var exception = new ExternalServiceException(serviceName, operation);
            exception.ErrorCode = "EXTERNAL_SERVICE_UNAVAILABLE";
            return exception;
        }

        /// <summary>
        /// Creates an exception for authentication failure
        /// </summary>
        public static ExternalServiceException AuthenticationFailed(string serviceName, string operation)
        {
            var exception = new ExternalServiceException(serviceName, operation, 401);
            exception.ErrorCode = "EXTERNAL_SERVICE_AUTH_FAILED";
            return exception;
        }

        /// <summary>
        /// Creates an exception for rate limiting
        /// </summary>
        public static ExternalServiceException RateLimited(string serviceName, string operation)
        {
            var exception = new ExternalServiceException(serviceName, operation, 429);
            exception.ErrorCode = "EXTERNAL_SERVICE_RATE_LIMITED";
            return exception;
        }
    }
}

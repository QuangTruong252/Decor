namespace DecorStore.API.Exceptions
{
    /// <summary>
    /// Exception thrown when database operations fail
    /// </summary>
    public class DatabaseException : Exception
    {
        /// <summary>
        /// The database operation that failed
        /// </summary>
        public string Operation { get; }

        /// <summary>
        /// The entity type involved in the operation
        /// </summary>
        public string? EntityType { get; }

        /// <summary>
        /// Error code for categorization
        /// </summary>
        public string ErrorCode { get; }

        public DatabaseException(string operation) 
            : base($"Database operation failed: {operation}")
        {
            Operation = operation;
            ErrorCode = "DATABASE_ERROR";
        }

        public DatabaseException(string operation, string entityType) 
            : base($"Database operation failed: {operation} for {entityType}")
        {
            Operation = operation;
            EntityType = entityType;
            ErrorCode = "DATABASE_ERROR";
        }

        public DatabaseException(string operation, Exception innerException) 
            : base($"Database operation failed: {operation}", innerException)
        {
            Operation = operation;
            ErrorCode = "DATABASE_ERROR";
        }

        public DatabaseException(string operation, string entityType, Exception innerException) 
            : base($"Database operation failed: {operation} for {entityType}", innerException)
        {
            Operation = operation;
            EntityType = entityType;
            ErrorCode = "DATABASE_ERROR";
        }

        public DatabaseException(string operation, string entityType, string errorCode, Exception innerException) 
            : base($"Database operation failed: {operation} for {entityType}", innerException)
        {
            Operation = operation;
            EntityType = entityType;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a database exception for constraint violations
        /// </summary>
        public static DatabaseException ConstraintViolation(string entityType, string constraint, Exception innerException)
        {
            return new DatabaseException(
                "Constraint violation", 
                entityType, 
                "DATABASE_CONSTRAINT_VIOLATION", 
                innerException);
        }

        /// <summary>
        /// Creates a database exception for concurrency conflicts
        /// </summary>
        public static DatabaseException ConcurrencyConflict(string entityType, Exception innerException)
        {
            return new DatabaseException(
                "Concurrency conflict", 
                entityType, 
                "DATABASE_CONCURRENCY_CONFLICT", 
                innerException);
        }

        /// <summary>
        /// Creates a database exception for connection failures
        /// </summary>
        public static DatabaseException ConnectionFailure(Exception innerException)
        {
            return new DatabaseException(
                "Connection failure",
                string.Empty,
                "DATABASE_CONNECTION_ERROR",
                innerException);
        }

        /// <summary>
        /// Creates a database exception for timeout errors
        /// </summary>
        public static DatabaseException Timeout(string operation, Exception innerException)
        {
            return new DatabaseException(
                operation,
                string.Empty,
                "DATABASE_TIMEOUT",
                innerException);
        }
    }
}

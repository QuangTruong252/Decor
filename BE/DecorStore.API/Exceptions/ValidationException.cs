namespace DecorStore.API.Exceptions
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Validation errors grouped by field name
        /// </summary>
        public Dictionary<string, string[]> ValidationErrors { get; }

        /// <summary>
        /// Error code for categorization
        /// </summary>
        public string ErrorCode { get; }

        public ValidationException() 
            : base("One or more validation errors occurred.")
        {
            ValidationErrors = new Dictionary<string, string[]>();
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(string message) 
            : base(message)
        {
            ValidationErrors = new Dictionary<string, string[]>();
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(Dictionary<string, string[]> validationErrors) 
            : base("One or more validation errors occurred.")
        {
            ValidationErrors = validationErrors;
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(string fieldName, string errorMessage) 
            : base("One or more validation errors occurred.")
        {
            ValidationErrors = new Dictionary<string, string[]>
            {
                { fieldName, new[] { errorMessage } }
            };
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(string fieldName, string[] errorMessages) 
            : base("One or more validation errors occurred.")
        {
            ValidationErrors = new Dictionary<string, string[]>
            {
                { fieldName, errorMessages }
            };
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(string message, Dictionary<string, string[]> validationErrors) 
            : base(message)
        {
            ValidationErrors = validationErrors;
            ErrorCode = "VALIDATION_ERROR";
        }

        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
            ValidationErrors = new Dictionary<string, string[]>();
            ErrorCode = "VALIDATION_ERROR";
        }

        /// <summary>
        /// Adds a validation error for a specific field
        /// </summary>
        public void AddError(string fieldName, string errorMessage)
        {
            if (ValidationErrors.ContainsKey(fieldName))
            {
                var existingErrors = ValidationErrors[fieldName].ToList();
                existingErrors.Add(errorMessage);
                ValidationErrors[fieldName] = existingErrors.ToArray();
            }
            else
            {
                ValidationErrors[fieldName] = new[] { errorMessage };
            }
        }

        /// <summary>
        /// Checks if there are any validation errors
        /// </summary>
        public bool HasErrors => ValidationErrors.Any();

        /// <summary>
        /// Gets all error messages as a flat list
        /// </summary>
        public IEnumerable<string> GetAllErrorMessages()
        {
            return ValidationErrors.SelectMany(kvp => kvp.Value);
        }
    }
}

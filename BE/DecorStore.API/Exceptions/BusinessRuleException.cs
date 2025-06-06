namespace DecorStore.API.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule is violated
    /// </summary>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        /// The business rule that was violated
        /// </summary>
        public string Rule { get; }

        /// <summary>
        /// Additional context about the violation
        /// </summary>
        public string? Context { get; }

        /// <summary>
        /// Error code for categorization
        /// </summary>
        public string ErrorCode { get; }

        public BusinessRuleException(string rule) 
            : base($"Business rule violation: {rule}")
        {
            Rule = rule;
            ErrorCode = "BUSINESS_RULE_VIOLATION";
        }

        public BusinessRuleException(string rule, string context) 
            : base($"Business rule violation: {rule}. Context: {context}")
        {
            Rule = rule;
            Context = context;
            ErrorCode = "BUSINESS_RULE_VIOLATION";
        }

        public BusinessRuleException(string rule, string context, string errorCode) 
            : base($"Business rule violation: {rule}. Context: {context}")
        {
            Rule = rule;
            Context = context;
            ErrorCode = errorCode;
        }

        public BusinessRuleException(string rule, Exception innerException) 
            : base($"Business rule violation: {rule}", innerException)
        {
            Rule = rule;
            ErrorCode = "BUSINESS_RULE_VIOLATION";
        }

        public BusinessRuleException(string rule, string context, Exception innerException) 
            : base($"Business rule violation: {rule}. Context: {context}", innerException)
        {
            Rule = rule;
            Context = context;
            ErrorCode = "BUSINESS_RULE_VIOLATION";
        }
    }
}

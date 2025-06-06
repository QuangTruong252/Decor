namespace DecorStore.API.Common
{
    /// <summary>
    /// Generic result pattern for consistent error handling and response management
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation is valid
    /// </summary>
    public bool IsValid => IsSuccess;

    /// <summary>
    /// Gets a value indicating whether the operation failed
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message (alias for Error property)
    /// </summary>
    public string? ErrorMessage => Error;

        /// <summary>
        /// Gets the data returned by the operation (only available on success)
        /// </summary>
        public T? Data { get; private set; }

        /// <summary>
        /// Gets the error message (only available on failure)
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Gets the error code for categorizing errors
        /// </summary>
        public string? ErrorCode { get; private set; }

        /// <summary>
        /// Gets additional error details or validation errors
        /// </summary>
        public IEnumerable<string>? ErrorDetails { get; private set; }

        private Result(bool isSuccess, T? data, string? error, string? errorCode = null, IEnumerable<string>? errorDetails = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
            ErrorCode = errorCode;
            ErrorDetails = errorDetails;
        }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <returns>A successful result</returns>
        public static Result<T> Success(T data)
        {
            return new Result<T>(true, data, null);
        }

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed result</returns>
        public static Result<T> Failure(string error)
        {
            return new Result<T>(false, default, error);
        }

        /// <summary>
        /// Creates a failed result with an error message and error code
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <returns>A failed result</returns>
        public static Result<T> Failure(string error, string errorCode)
        {
            return new Result<T>(false, default, error, errorCode);
        }

        /// <summary>
        /// Creates a failed result with an error message, error code, and additional details
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDetails">Additional error details</param>
        /// <returns>A failed result</returns>
        public static Result<T> Failure(string error, string errorCode, IEnumerable<string> errorDetails)
        {
            return new Result<T>(false, default, error, errorCode, errorDetails);
        }

        /// <summary>
        /// Creates a validation failed result with multiple validation errors
        /// </summary>
        /// <param name="validationErrors">The validation errors</param>
        /// <returns>A failed result with validation errors</returns>
        public static Result<T> ValidationFailure(IEnumerable<string> validationErrors)
        {
            return new Result<T>(false, default, "Validation failed", "VALIDATION_ERROR", validationErrors);
        }

        /// <summary>
        /// Creates a not found result
        /// </summary>
        /// <param name="entityName">The name of the entity that was not found</param>
        /// <returns>A failed result indicating entity not found</returns>
        public static Result<T> NotFound(string entityName = "Entity")
        {
            return new Result<T>(false, default, $"{entityName} not found", "NOT_FOUND");
        }

        /// <summary>
        /// Creates an unauthorized result
        /// </summary>
        /// <param name="message">The unauthorized message</param>
        /// <returns>A failed result indicating unauthorized access</returns>
        public static Result<T> Unauthorized(string message = "Unauthorized access")
        {
            return new Result<T>(false, default, message, "UNAUTHORIZED");
        }

        /// <summary>
        /// Creates a forbidden result
        /// </summary>
        /// <param name="message">The forbidden message</param>
        /// <returns>A failed result indicating forbidden access</returns>
        public static Result<T> Forbidden(string message = "Access forbidden")
        {
            return new Result<T>(false, default, message, "FORBIDDEN");
        }

        /// <summary>
        /// Implicit conversion from data to successful result
        /// </summary>
        /// <param name="data">The data</param>
        public static implicit operator Result<T>(T data)
        {
            return Success(data);
        }

        /// <summary>
        /// Implicit conversion from string to failed result
        /// </summary>
        /// <param name="error">The error message</param>
        public static implicit operator Result<T>(string error)
        {
            return Failure(error);
        }

        /// <summary>
        /// Operator overloading for ! (not) to check IsSuccess
        /// </summary>
        public static bool operator !(Result<T> result)
        {
            return !result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion to bool to check IsSuccess
        /// </summary>
        public static implicit operator bool(Result<T> result)
        {
            return result.IsSuccess;
        }
    }

    /// <summary>
    /// Non-generic result for operations that don't return data
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the operation is valid
        /// </summary>
        public bool IsValid => IsSuccess;

        /// <summary>
        /// Gets a value indicating whether the operation failed
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error message (alias for Error property)
        /// </summary>
        public string? ErrorMessage => Error;

        /// <summary>
        /// Gets the error message (only available on failure)
        /// </summary>
        public string? Error { get; private set; }

        /// <summary>
        /// Gets the error code for categorizing errors
        /// </summary>
        public string? ErrorCode { get; private set; }

        /// <summary>
        /// Gets additional error details or validation errors
        /// </summary>
        public IEnumerable<string>? ErrorDetails { get; private set; }

        private Result(bool isSuccess, string? error, string? errorCode = null, IEnumerable<string>? errorDetails = null)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorCode = errorCode;
            ErrorDetails = errorDetails;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <returns>A successful result</returns>
        public static Result Success()
        {
            return new Result(true, null);
        }

        /// <summary>
        /// Creates a failed result with an error message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A failed result</returns>
        public static Result Failure(string error)
        {
            return new Result(false, error);
        }

        /// <summary>
        /// Creates a failed result with an error message and error code
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <returns>A failed result</returns>
        public static Result Failure(string error, string errorCode)
        {
            return new Result(false, error, errorCode);
        }

        /// <summary>
        /// Creates a failed result with an error message, error code, and additional details
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDetails">Additional error details</param>
        /// <returns>A failed result</returns>
        public static Result Failure(string error, string errorCode, IEnumerable<string> errorDetails)
        {
            return new Result(false, error, errorCode, errorDetails);
        }

        /// <summary>
        /// Creates a validation failed result with multiple validation errors
        /// </summary>
        /// <param name="validationErrors">The validation errors</param>
        /// <returns>A failed result with validation errors</returns>
        public static Result ValidationFailure(IEnumerable<string> validationErrors)
        {
            return new Result(false, "Validation failed", "VALIDATION_ERROR", validationErrors);
        }

        /// <summary>
        /// Creates a not found result
        /// </summary>
        /// <param name="entityName">The name of the entity that was not found</param>
        /// <returns>A failed result indicating entity not found</returns>
        public static Result NotFound(string entityName = "Entity")
        {
            return new Result(false, $"{entityName} not found", "NOT_FOUND");
        }

        /// <summary>
        /// Creates an unauthorized result
        /// </summary>
        /// <param name="message">The unauthorized message</param>
        /// <returns>A failed result indicating unauthorized access</returns>
        public static Result Unauthorized(string message = "Unauthorized access")
        {
            return new Result(false, message, "UNAUTHORIZED");
        }

        /// <summary>
        /// Creates a forbidden result
        /// </summary>
        /// <param name="message">The forbidden message</param>
        /// <returns>A failed result indicating forbidden access</returns>
        public static Result Forbidden(string message = "Access forbidden")
        {
            return new Result(false, message, "FORBIDDEN");
        }

        /// <summary>
        /// Operator overloading for ! (not) to check IsSuccess
        /// </summary>
        public static bool operator !(Result result)
        {
            return !result.IsSuccess;
        }

        /// <summary>
        /// Implicit conversion to bool to check IsSuccess
        /// </summary>
        public static implicit operator bool(Result result)
        {
            return result.IsSuccess;
        }
    }

    /// <summary>
    /// Extension methods for Result pattern
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Executes an action if the result is successful
        /// </summary>
        /// <typeparam name="T">The result data type</typeparam>
        /// <param name="result">The result</param>
        /// <param name="onSuccess">The action to execute on success</param>
        /// <returns>The original result</returns>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> onSuccess)
        {
            if (result.IsSuccess && result.Data != null)
            {
                onSuccess(result.Data);
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is failed
        /// </summary>
        /// <typeparam name="T">The result data type</typeparam>
        /// <param name="result">The result</param>
        /// <param name="onFailure">The action to execute on failure</param>
        /// <returns>The original result</returns>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<string> onFailure)
        {
            if (result.IsFailure && result.Error != null)
            {
                onFailure(result.Error);
            }
            return result;
        }

        /// <summary>
        /// Maps the result data to a different type
        /// </summary>
        /// <typeparam name="T">The source data type</typeparam>
        /// <typeparam name="U">The target data type</typeparam>
        /// <param name="result">The source result</param>
        /// <param name="mapper">The mapping function</param>
        /// <returns>A result with the mapped data</returns>
        public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
        {
            if (result.IsFailure)
            {
                return Result<U>.Failure(result.Error!, result.ErrorCode!, result.ErrorDetails!);
            }

            try
            {
                var mappedData = mapper(result.Data!);
                return Result<U>.Success(mappedData);
            }
            catch (Exception ex)
            {
                return Result<U>.Failure($"Mapping failed: {ex.Message}", "MAPPING_ERROR");
            }
        }

        /// <summary>
        /// Adds LINQ extensions for Result<IEnumerable<T>>
        /// </summary>
        public static int Count<T>(this Result<IEnumerable<T>> result)
        {
            return result.IsSuccess ? result.Data?.Count() ?? 0 : 0;
        }

        public static Result<IEnumerable<U>> Select<T, U>(this Result<IEnumerable<T>> result, Func<T, U> selector)
        {
            if (result.IsFailure)
            {
                return Result<IEnumerable<U>>.Failure(result.Error!, result.ErrorCode!, result.ErrorDetails!);
            }

            try
            {
                var mappedData = result.Data!.Select(selector);
                return Result<IEnumerable<U>>.Success(mappedData);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<U>>.Failure($"Mapping failed: {ex.Message}", "MAPPING_ERROR");
            }
        }

        public static Result<List<U>> Select<T, U>(this Result<List<T>> result, Func<T, U> selector)
        {
            if (result.IsFailure)
            {
                return Result<List<U>>.Failure(result.Error!, result.ErrorCode!, result.ErrorDetails!);
            }

            try
            {
                var mappedData = result.Data!.Select(selector).ToList();
                return Result<List<U>>.Success(mappedData);
            }
            catch (Exception ex)
            {
                return Result<List<U>>.Failure($"Mapping failed: {ex.Message}", "MAPPING_ERROR");
            }
        }

        public static int Count<T>(this Result<List<T>> result)
        {
            return result.IsSuccess ? result.Data?.Count ?? 0 : 0;
        }
    }
}

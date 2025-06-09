using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Common;
using DecorStore.API.Models;
using System.Diagnostics;

namespace DecorStore.API.Controllers.Base
{
    /// <summary>
    /// Base controller providing consistent response handling and common functionality
    /// </summary>    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<BaseController>? _logger;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected BaseController() 
        {
            // Parameterless constructor for controllers that don't need logging
            _logger = null;
        }

        /// <summary>
        /// Handles Result<T> and returns appropriate HTTP response
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="result">The result to handle</param>
        /// <returns>Appropriate ActionResult based on result state</returns>
        protected ActionResult<T> HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return HandleFailureResult<T>(result);
        }

        /// <summary>
        /// Validates the model state and returns standardized error response
        /// </summary>
        protected IActionResult ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                var errorCodes = new List<string>();

                foreach (var kvp in ModelState.Where(x => x.Value?.Errors.Count > 0))
                {
                    var fieldErrors = new List<string>();
                    var fieldErrorCodes = new List<string>();

                    foreach (var error in kvp.Value!.Errors)
                    {
                        // Handle FluentValidation errors which may include error codes
                        if (!string.IsNullOrEmpty(error.ErrorMessage))
                        {
                            fieldErrors.Add(error.ErrorMessage);
                            
                            // Extract error code if it follows the pattern [CODE] Message
                            var match = System.Text.RegularExpressions.Regex.Match(
                                error.ErrorMessage, @"^\[([A-Z_]+)\]\s*(.+)$");
                            
                            if (match.Success)
                            {
                                fieldErrorCodes.Add(match.Groups[1].Value);
                                // Update the error message to remove the code prefix
                                fieldErrors[^1] = match.Groups[2].Value;
                            }
                        }
                    }

                    if (fieldErrors.Any())
                    {
                        errors[kvp.Key] = fieldErrors.ToArray();
                        errorCodes.AddRange(fieldErrorCodes);
                    }
                }

                // Determine primary error code
                var primaryErrorCode = errorCodes.FirstOrDefault() ?? "VALIDATION_ERROR";
                
                // Determine severity based on error types
                var severity = DetermineSeverity(errorCodes);

                // Generate contextual suggested actions
                var suggestedActions = GenerateSuggestedActions(errors);

                var errorResponse = new ErrorResponse
                {
                    CorrelationId = GetCorrelationId(),
                    ErrorCode = primaryErrorCode,
                    Message = GetValidationErrorMessage(errors.Count),
                    Details = $"Validation failed for {errors.Count} field(s). Please review and correct the highlighted issues.",
                    Timestamp = DateTime.UtcNow,
                    Path = Request.Path,
                    ValidationErrors = errors,
                    Severity = severity,
                    SuggestedActions = suggestedActions.ToArray(),
                    Metadata = new Dictionary<string, object>
                    {
                        { "TotalErrors", errors.Values.Sum(e => e.Length) },
                        { "FieldsWithErrors", errors.Count },
                        { "ErrorCodes", errorCodes.Distinct().ToArray() }
                    }
                };

                return BadRequest(errorResponse);
            }

            return null!; // Model state is valid
        }

        /// <summary>
        /// Determines error severity based on error codes
        /// </summary>
        private static ErrorSeverity DetermineSeverity(List<string> errorCodes)
        {
            if (errorCodes.Any(code => code.Contains("SECURITY") || code.Contains("UNAUTHORIZED")))
                return ErrorSeverity.Critical;
            
            if (errorCodes.Any(code => code.Contains("DUPLICATE") || code.Contains("CONFLICT")))
                return ErrorSeverity.Error;
            
            if (errorCodes.Any(code => code.Contains("FORMAT") || code.Contains("INVALID")))
                return ErrorSeverity.Warning;
            
            return ErrorSeverity.Error;
        }

        /// <summary>
        /// Generates contextual suggested actions based on validation errors
        /// </summary>
        private static List<string> GenerateSuggestedActions(Dictionary<string, string[]> errors)
        {
            var actions = new List<string>();

            foreach (var field in errors.Keys)
            {
                var fieldName = field.ToLowerInvariant();
                var fieldErrors = errors[field];

                if (fieldErrors.Any(e => e.Contains("required", StringComparison.OrdinalIgnoreCase)))
                {
                    actions.Add($"Provide a value for {field}");
                }
                else if (fieldErrors.Any(e => e.Contains("format", StringComparison.OrdinalIgnoreCase)))
                {
                    actions.Add($"Check the format of {field}");
                }
                else if (fieldErrors.Any(e => e.Contains("length", StringComparison.OrdinalIgnoreCase)))
                {
                    actions.Add($"Adjust the length of {field}");
                }
                else if (fieldErrors.Any(e => e.Contains("exists", StringComparison.OrdinalIgnoreCase)))
                {
                    actions.Add($"Use a different value for {field}");
                }
                else if (fieldErrors.Any(e => e.Contains("range", StringComparison.OrdinalIgnoreCase)))
                {
                    actions.Add($"Ensure {field} is within the valid range");
                }
            }

            if (!actions.Any())
            {
                actions.Add("Please review and correct the validation errors");
                actions.Add("Ensure all required fields are provided");
                actions.Add("Check that all values meet the specified requirements");
            }

            return actions.Distinct().Take(5).ToList(); // Limit to 5 most relevant actions
        }

        /// <summary>
        /// Gets appropriate validation error message based on error count
        /// </summary>
        private static string GetValidationErrorMessage(int errorCount)
        {
            return errorCount switch
            {
                1 => "A validation error occurred",
                <= 5 => $"{errorCount} validation errors occurred",
                _ => $"Multiple validation errors occurred ({errorCount} errors)"
            };
        }

        /// <summary>
        /// Handles paginated results
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="result">The paginated result</param>
        /// <returns>ActionResult with pagination headers</returns>
        protected ActionResult<T> HandlePagedResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                AddCorrelationId();
                return Ok(result.Data);
            }

            return HandleFailureResult<T>(result);
        }

        private ActionResult<T> HandleFailureResult<T>(Result<T> result)
        {
            var correlationId = GetOrCreateCorrelationId();
            
            LogError(result, correlationId);

            var errorResponse = CreateErrorResponse(result, correlationId);

            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(errorResponse),
                "UNAUTHORIZED" => Unauthorized(errorResponse),
                "FORBIDDEN" => StatusCode(403, errorResponse),
                "VALIDATION_ERROR" => BadRequest(errorResponse),
                _ => BadRequest(errorResponse)
            };
        }

        private ActionResult HandleFailureResult(Result result)
        {
            var correlationId = GetOrCreateCorrelationId();
            
            LogError(result, correlationId);

            var errorResponse = CreateErrorResponse(result, correlationId);

            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(errorResponse),
                "UNAUTHORIZED" => Unauthorized(errorResponse),
                "FORBIDDEN" => StatusCode(403, errorResponse),
                "VALIDATION_ERROR" => BadRequest(errorResponse),
                _ => BadRequest(errorResponse)
            };
        }

        private object CreateErrorResponse(Result result, string correlationId)
        {
            var response = new
            {
                Error = result.Error,
                ErrorCode = result.ErrorCode,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Details = result.ErrorDetails
            };

            return response;
        }

        private object CreateErrorResponse<T>(Result<T> result, string correlationId)
        {
            var response = new
            {
                Error = result.Error,
                ErrorCode = result.ErrorCode,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Details = result.ErrorDetails
            };

            return response;
        }

        private void LogError(Result result, string correlationId)
        {
            if (_logger != null)
            {
                _logger.LogWarning(
                    "Request failed with error: {Error}, ErrorCode: {ErrorCode}, CorrelationId: {CorrelationId}",
                    result.Error,
                    result.ErrorCode,
                    correlationId);
            }
        }

        private void LogError<T>(Result<T> result, string correlationId)
        {
            if (_logger != null)
            {
                _logger.LogWarning(
                    "Request failed with error: {Error}, ErrorCode: {ErrorCode}, CorrelationId: {CorrelationId}",
                    result.Error,
                    result.ErrorCode,
                    correlationId);
            }
        }

        private string GetOrCreateCorrelationId()
        {
            var correlationId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
            
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            return correlationId;
        }

        private void AddCorrelationId()
        {
            var correlationId = GetOrCreateCorrelationId();
              if (HttpContext?.Response?.Headers != null && !HttpContext.Response.Headers.ContainsKey("X-Correlation-ID"))
            {
                HttpContext.Response.Headers["X-Correlation-ID"] = correlationId;
            }
        }

        /// <summary>
        /// Gets the current user ID from claims
        /// </summary>
        /// <returns>The current user ID or null if not authenticated</returns>
        protected string? GetCurrentUserId()
        {
            return User?.FindFirst("sub")?.Value ?? User?.FindFirst("id")?.Value;
        }

        /// <summary>
        /// Gets the current user email from claims
        /// </summary>
        /// <returns>The current user email or null if not authenticated</returns>
        protected string? GetCurrentUserEmail()
        {
            return User?.FindFirst("email")?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        protected bool HasRole(string role)
        {
            return User?.IsInRole(role) ?? false;
        }


        /// <summary>
        /// Creates a validation error response for invalid model state
        /// </summary>
        /// <returns>BadRequest with validation errors</returns>
        protected IActionResult HandleValidationErrors()
        {
            var validationResult = ValidateModelState();
            return validationResult ?? Ok(); // Return the validation result directly, or Ok() if valid
        }

        /// <summary>
        /// Gets the correlation ID from the current request context
        /// </summary>
        /// <returns>The correlation ID</returns>
        protected string GetCorrelationId()
        {
            return GetOrCreateCorrelationId();
        }

        /// <summary>
        /// Handles create operations and returns appropriate HTTP response
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="result">The result to handle</param>
        /// <returns>Created response on success, error response on failure</returns>
        protected ActionResult<T> HandleCreateResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                AddCorrelationId();
                return Created(string.Empty, result.Data);
            }

            return HandleFailureResult<T>(result);
        }

        /// <summary>
        /// Handles non-generic result operations
        /// </summary>
        /// <param name="result">The result to handle</param>
        /// <returns>Appropriate ActionResult based on result state</returns>
        protected ActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                AddCorrelationId();
                return Ok();
            }

            return HandleFailureResult(result);
        }
    }
}

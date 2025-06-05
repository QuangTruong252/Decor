using Microsoft.AspNetCore.Mvc;
using DecorStore.API.Common;
using System.Diagnostics;

namespace DecorStore.API.Controllers.Base
{
    /// <summary>
    /// Base controller providing consistent response handling and common functionality
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<BaseController> _logger;

        protected BaseController(ILogger<BaseController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected BaseController() 
        {
            // Parameterless constructor for controllers that don't need logging
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
        /// Handles non-generic Result and returns appropriate HTTP response
        /// </summary>
        /// <param name="result">The result to handle</param>
        /// <returns>Appropriate ActionResult based on result state</returns>
        protected ActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleFailureResult(result);
        }

        /// <summary>
        /// Handles Result<T> for create operations with location header
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="result">The result to handle</param>
        /// <param name="actionName">The action name for location header</param>
        /// <param name="routeValues">The route values for location header</param>
        /// <returns>Appropriate ActionResult based on result state</returns>
        protected ActionResult<T> HandleCreateResult<T>(Result<T> result, string actionName, object? routeValues = null)
        {
            if (result.IsSuccess)
            {
                return CreatedAtAction(actionName, routeValues, result.Data);
            }

            return HandleFailureResult<T>(result);
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
                HttpContext.Response.Headers.Add("X-Correlation-ID", correlationId);
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
        /// Validates model state and returns validation result
        /// </summary>
        /// <returns>Result indicating validation success or failure</returns>
        protected Result ValidateModelState()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return Result.ValidationFailure(errors);
            }

            return Result.Success();
        }

        /// <summary>
        /// Creates a validation error response for invalid model state
        /// </summary>
        /// <returns>BadRequest with validation errors</returns>
        protected ActionResult HandleValidationErrors()
        {
            var validationResult = ValidateModelState();
            return HandleResult(validationResult);
        }
    }
}

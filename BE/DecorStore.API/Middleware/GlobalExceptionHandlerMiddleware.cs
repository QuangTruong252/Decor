using DecorStore.API.Exceptions;
using DecorStore.API.Extensions;
using DecorStore.API.Models;
using DecorStore.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;
using ValidationException = DecorStore.API.Exceptions.ValidationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace DecorStore.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware that catches and processes all unhandled exceptions
    /// </summary>
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly DecorStore.API.Interfaces.Services.ICorrelationIdService _correlationIdService;

        public GlobalExceptionHandlerMiddleware(
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            DecorStore.API.Interfaces.Services.ICorrelationIdService correlationIdService)
        {
            _logger = logger;
            _correlationIdService = correlationIdService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            
            // Check if response has already started
            if (context.Response.HasStarted)
            {
                // If response has started, we can only log the exception
                _logger.LogError(exception, 
                    "Exception occurred but response already started. CorrelationId: {CorrelationId}", 
                    correlationId);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;

            // Log the exception with correlation ID
            LogException(exception, correlationId, context);

            // Create appropriate error response
            var errorResponse = CreateErrorResponse(exception, correlationId, path);
            var statusCode = GetStatusCode(exception);

            try
            {
                // Clear any existing response content
                context.Response.Clear();
                
                // Set response properties
                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";

                // Add correlation ID to response headers
                context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);

                // Serialize and write response
                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await context.Response.WriteAsync(jsonResponse);
            }
            catch (Exception writeException)
            {
                // If we can't write to the response, just log the error
                _logger.LogError(writeException, 
                    "Failed to write exception response. Original exception: {OriginalException}, CorrelationId: {CorrelationId}", 
                    exception.Message, correlationId);
            }
        }

        private void LogException(Exception exception, string correlationId, HttpContext context)
        {
            var logLevel = GetLogLevel(exception);
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var logData = new
            {
                CorrelationId = correlationId,
                ExceptionType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                UserId = userId,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                RequestPath = context.Request.Path.Value,
                RequestMethod = context.Request.Method,
                QueryString = context.Request.QueryString.Value,
                InnerException = exception.InnerException?.Message
            };

            _logger.Log(logLevel,
                exception,
                "Unhandled exception occurred [{CorrelationId}]: {ExceptionType} - {Message}. Context: {@LogData}",
                correlationId,
                exception.GetType().Name,
                exception.Message,
                logData);

            // Log additional details for specific exception types
            LogSpecificExceptionDetails(exception, correlationId);
        }

        private void LogSpecificExceptionDetails(Exception exception, string correlationId)
        {
            switch (exception)
            {
                case ValidationException validationEx:
                    _logger.LogWarning(
                        "Validation errors [{CorrelationId}]: {@ValidationErrors}",
                        correlationId,
                        validationEx.ValidationErrors);
                    break;

                case BusinessRuleException businessEx:
                    _logger.LogWarning(
                        "Business rule violation [{CorrelationId}]: Rule='{Rule}', Context='{Context}'",
                        correlationId,
                        businessEx.Rule,
                        businessEx.Context);
                    break;

                case DatabaseException dbEx:
                    _logger.LogError(
                        "Database operation failed [{CorrelationId}]: Operation='{Operation}', EntityType='{EntityType}'",
                        correlationId,
                        dbEx.Operation,
                        dbEx.EntityType);
                    break;

                case ExternalServiceException serviceEx:
                    _logger.LogError(
                        "External service call failed [{CorrelationId}]: Service='{ServiceName}', Operation='{Operation}', StatusCode={StatusCode}",
                        correlationId,
                        serviceEx.ServiceName,
                        serviceEx.Operation,
                        serviceEx.StatusCode);
                    break;

                case DbUpdateConcurrencyException concurrencyEx:
                    _logger.LogWarning(
                        "Database concurrency conflict [{CorrelationId}]: {EntityCount} entities affected",
                        correlationId,
                        concurrencyEx.Entries.Count);
                    break;

                case DbUpdateException dbUpdateEx:
                    _logger.LogError(
                        "Database update failed [{CorrelationId}]: {EntityCount} entities affected",
                        correlationId,
                        dbUpdateEx.Entries.Count);
                    break;
            }
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string correlationId, string path)
        {
            return exception switch
            {
                ValidationException validationEx => ErrorResponse.ValidationError(
                    correlationId, path, validationEx.ValidationErrors),

                FluentValidation.ValidationException fluentValidationEx => ErrorResponse.ValidationError(
                    correlationId, path, ConvertFluentValidationErrors(fluentValidationEx)),

                NotFoundException => ErrorResponse.NotFound(
                    correlationId, path, "resource"),

                UnauthorizedException => ErrorResponse.Unauthorized(
                    correlationId, path),

                BusinessRuleException businessEx => ErrorResponse.BusinessRuleViolation(
                    correlationId, path, businessEx.Rule, businessEx.Context ?? string.Empty),

                DatabaseException dbEx => CreateDatabaseErrorResponse(dbEx, correlationId, path),

                ExternalServiceException serviceEx => CreateExternalServiceErrorResponse(serviceEx, correlationId, path),

                DbUpdateConcurrencyException => new ErrorResponse
                {
                    CorrelationId = correlationId,
                    ErrorCode = "CONCURRENCY_CONFLICT",
                    Message = "The record was modified by another user. Please refresh and try again.",
                    Path = path,
                    Severity = ErrorSeverity.Warning,
                    SuggestedActions = new[] { "Refresh the data and try again." }
                },

                TimeoutException => new ErrorResponse
                {
                    CorrelationId = correlationId,
                    ErrorCode = "TIMEOUT",
                    Message = "The operation timed out. Please try again.",
                    Path = path,
                    Severity = ErrorSeverity.Warning,
                    SuggestedActions = new[] { "Please try again in a few moments." }
                },

                ArgumentException argEx => new ErrorResponse
                {
                    CorrelationId = correlationId,
                    ErrorCode = "INVALID_ARGUMENT",
                    Message = "Invalid argument provided.",
                    Path = path,
                    Details = argEx.Message,
                    Severity = ErrorSeverity.Warning
                },

                _ => ErrorResponse.InternalServerError(correlationId, path, 
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                        ? exception.Message 
                        : null)
            };
        }

        private Dictionary<string, string[]> ConvertFluentValidationErrors(FluentValidation.ValidationException ex)
        {
            return ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
        }

        private ErrorResponse CreateDatabaseErrorResponse(DatabaseException dbEx, string correlationId, string path)
        {
            var message = dbEx.ErrorCode switch
            {
                "DATABASE_CONSTRAINT_VIOLATION" => "A data constraint was violated. Please check your input.",
                "DATABASE_CONCURRENCY_CONFLICT" => "The record was modified by another user. Please refresh and try again.",
                "DATABASE_CONNECTION_ERROR" => "Unable to connect to the database. Please try again later.",
                "DATABASE_TIMEOUT" => "The database operation timed out. Please try again.",
                _ => "A database error occurred while processing your request."
            };

            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = dbEx.ErrorCode,
                Message = message,
                Path = path,
                Severity = ErrorSeverity.Error,
                SuggestedActions = new[] { "Please try again or contact support if the problem persists." }
            };
        }

        private ErrorResponse CreateExternalServiceErrorResponse(ExternalServiceException serviceEx, string correlationId, string path)
        {
            var message = serviceEx.ErrorCode switch
            {
                "EXTERNAL_SERVICE_TIMEOUT" => $"The {serviceEx.ServiceName} service is currently unavailable. Please try again later.",
                "EXTERNAL_SERVICE_UNAVAILABLE" => $"The {serviceEx.ServiceName} service is temporarily unavailable.",
                "EXTERNAL_SERVICE_AUTH_FAILED" => $"Authentication failed with {serviceEx.ServiceName} service.",
                "EXTERNAL_SERVICE_RATE_LIMITED" => $"Rate limit exceeded for {serviceEx.ServiceName} service.",
                _ => $"An error occurred while communicating with {serviceEx.ServiceName} service."
            };

            return new ErrorResponse
            {
                CorrelationId = correlationId,
                ErrorCode = serviceEx.ErrorCode,
                Message = message,
                Path = path,
                Severity = ErrorSeverity.Error,
                SuggestedActions = new[] { "Please try again later." }
            };
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                FluentValidation.ValidationException => HttpStatusCode.BadRequest,
                NotFoundException => HttpStatusCode.NotFound,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                BusinessRuleException => HttpStatusCode.BadRequest,
                DatabaseException dbEx when dbEx.ErrorCode == "DATABASE_CONSTRAINT_VIOLATION" => HttpStatusCode.Conflict,
                DatabaseException => HttpStatusCode.InternalServerError,
                ExternalServiceException serviceEx when serviceEx.StatusCode.HasValue => 
                    (HttpStatusCode)serviceEx.StatusCode.Value,
                ExternalServiceException => HttpStatusCode.BadGateway,
                DbUpdateConcurrencyException => HttpStatusCode.Conflict,
                TimeoutException => HttpStatusCode.RequestTimeout,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private static LogLevel GetLogLevel(Exception exception)
        {
            return exception switch
            {
                ValidationException => LogLevel.Warning,
                FluentValidation.ValidationException => LogLevel.Warning,
                NotFoundException => LogLevel.Warning,
                UnauthorizedException => LogLevel.Warning,
                BusinessRuleException => LogLevel.Warning,
                DatabaseException => LogLevel.Error,
                ExternalServiceException => LogLevel.Error,
                DbUpdateConcurrencyException => LogLevel.Warning,
                TimeoutException => LogLevel.Warning,
                ArgumentException => LogLevel.Warning,
                _ => LogLevel.Error
            };
        }
    }
}

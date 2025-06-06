using Microsoft.Extensions.Options;

namespace DecorStore.API.Configuration.Validators
{
    /// <summary>
    /// Validator for ApiSettings configuration
    /// </summary>
    public class ApiSettingsValidator : IValidateOptions<ApiSettings>
    {
        public ValidateOptionsResult Validate(string? name, ApiSettings options)
        {
            var failures = new List<string>();

            // Validate API version format
            if (string.IsNullOrEmpty(options.DefaultVersion))
            {
                failures.Add("API DefaultVersion cannot be null or empty");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(options.DefaultVersion, @"^v\d+(\.\d+)?$"))
            {
                failures.Add("API DefaultVersion must be in format 'v1' or 'v1.0'");
            }

            // Validate supported versions
            if (options.SupportedVersions == null || !options.SupportedVersions.Any())
            {
                failures.Add("API SupportedVersions cannot be null or empty");
            }
            else
            {
                foreach (var version in options.SupportedVersions)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^v\d+(\.\d+)?$"))
                    {
                        failures.Add($"API SupportedVersion '{version}' must be in format 'v1' or 'v1.0'");
                    }
                }
            }

            // Validate rate limiting settings
            if (options.RequestsPerMinute <= 0)
            {
                failures.Add("API RequestsPerMinute must be greater than 0");
            }

            if (options.BurstLimit <= 0)
            {
                failures.Add("API BurstLimit must be greater than 0");
            }

            // Validate CORS settings
            if (options.AllowedOrigins == null || !options.AllowedOrigins.Any())
            {
                failures.Add("API AllowedOrigins cannot be null or empty");
            }
            else
            {
                foreach (var origin in options.AllowedOrigins)
                {
                    if (string.IsNullOrWhiteSpace(origin))
                    {
                        failures.Add("API AllowedOrigins cannot contain null or empty values");
                        break;
                    }
                }
            }

            // Validate allowed headers
            if (options.AllowedHeaders == null || !options.AllowedHeaders.Any())
            {
                failures.Add("API AllowedHeaders cannot be null or empty");
            }

            // Validate allowed methods
            if (options.AllowedMethods == null || !options.AllowedMethods.Any())
            {
                failures.Add("API AllowedMethods cannot be null or empty");
            }
            else
            {
                var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD" };
                foreach (var method in options.AllowedMethods)
                {
                    if (!validMethods.Contains(method.ToUpper()))
                    {
                        failures.Add($"API AllowedMethod '{method}' is not a valid HTTP method");
                    }
                }
            }

            // Validate Swagger settings
            if (options.EnableSwagger)
            {
                if (string.IsNullOrEmpty(options.SwaggerEndpoint))
                {
                    failures.Add("API SwaggerEndpoint cannot be null or empty when Swagger is enabled");
                }

                if (string.IsNullOrEmpty(options.SwaggerTitle))
                {
                    failures.Add("API SwaggerTitle cannot be null or empty when Swagger is enabled");
                }
            }

            // Validate logging settings
            var validLogLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };
            if (!validLogLevels.Contains(options.DefaultLogLevel))
            {
                failures.Add($"API DefaultLogLevel '{options.DefaultLogLevel}' is not a valid log level");
            }

            if (!validLogLevels.Contains(options.MicrosoftLogLevel))
            {
                failures.Add($"API MicrosoftLogLevel '{options.MicrosoftLogLevel}' is not a valid log level");
            }

            // Validate performance settings
            if (options.RequestTimeoutSeconds <= 0)
            {
                failures.Add("API RequestTimeoutSeconds must be greater than 0");
            }

            if (options.MaxRequestBodySizeMB <= 0)
            {
                failures.Add("API MaxRequestBodySizeMB must be greater than 0");
            }

            if (options.DefaultCacheDurationSeconds <= 0)
            {
                failures.Add("API DefaultCacheDurationSeconds must be greater than 0");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}

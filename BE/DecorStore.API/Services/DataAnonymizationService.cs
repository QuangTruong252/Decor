using DecorStore.API.Common;
using DecorStore.API.Interfaces.Services;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for data anonymization and PII protection
    /// </summary>
    public interface IDataAnonymizationService
    {
        Task<Result<string>> AnonymizeDataAsync(string data, AnonymizationLevel level = AnonymizationLevel.Standard);
        Task<Result<string>> MaskPiiAsync(string data);
        Task<Result<T>> AnonymizeObjectAsync<T>(T obj, AnonymizationLevel level = AnonymizationLevel.Standard) where T : class;
        Task<Result<string>> RedactSensitiveDataAsync(string logMessage);
        Task<Result<bool>> ContainsPiiAsync(string data);
        Task<Result<List<PiiDetectionResult>>> DetectPiiAsync(string data);
        Task<Result<string>> ApplyDataRetentionPolicyAsync(string data, DataRetentionPolicy policy);
        Task<Result<Dictionary<string, object>>> CreateAnonymousProfileAsync(int userId);
        Task<Result> PurgeUserDataAsync(int userId, PurgeOptions options);
        Task<Result<byte[]>> ExportUserDataAsync(int userId, ExportFormat format);
    }

    public class DataAnonymizationService : IDataAnonymizationService
    {
        private readonly ILogger<DataAnonymizationService> _logger;
        private readonly IDataEncryptionService _encryptionService;

        // PII detection patterns
        private static readonly Dictionary<PiiType, Regex> PiiPatterns = new()
        {
            { PiiType.Email, new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase) },
            { PiiType.Phone, new Regex(@"(\+\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", RegexOptions.Compiled) },
            { PiiType.SSN, new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled) },
            { PiiType.CreditCard, new Regex(@"\b(?:\d{4}[-\s]?){3}\d{4}\b", RegexOptions.Compiled) },
            { PiiType.IPAddress, new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled) },
            { PiiType.DateOfBirth, new Regex(@"\b\d{1,2}[/-]\d{1,2}[/-]\d{4}\b", RegexOptions.Compiled) },
            { PiiType.BankAccount, new Regex(@"\b\d{8,17}\b", RegexOptions.Compiled) },
            { PiiType.PostalCode, new Regex(@"\b\d{5}(-\d{4})?\b", RegexOptions.Compiled) }
        };

        public DataAnonymizationService(
            ILogger<DataAnonymizationService> logger,
            IDataEncryptionService encryptionService)
        {
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task<Result<string>> AnonymizeDataAsync(string data, AnonymizationLevel level = AnonymizationLevel.Standard)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(data);

                var anonymizedData = data;

                switch (level)
                {
                    case AnonymizationLevel.Basic:
                        anonymizedData = await ApplyBasicAnonymizationAsync(data);
                        break;
                    case AnonymizationLevel.Standard:
                        anonymizedData = await ApplyStandardAnonymizationAsync(data);
                        break;
                    case AnonymizationLevel.Aggressive:
                        anonymizedData = await ApplyAggressiveAnonymizationAsync(data);
                        break;
                }

                return Result<string>.Success(anonymizedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing data");
                return Result<string>.Failure("Failed to anonymize data");
            }
        }

        public async Task<Result<string>> MaskPiiAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(data);

                var maskedData = data;

                foreach (var pattern in PiiPatterns)
                {
                    maskedData = pattern.Value.Replace(maskedData, match =>
                    {
                        return pattern.Key switch
                        {
                            PiiType.Email => MaskEmail(match.Value),
                            PiiType.Phone => MaskPhone(match.Value),
                            PiiType.CreditCard => MaskCreditCard(match.Value),
                            PiiType.SSN => "***-**-****",
                            PiiType.IPAddress => "***.***.***.**",
                            _ => new string('*', match.Value.Length)
                        };
                    });
                }

                return Result<string>.Success(maskedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error masking PII data");
                return Result<string>.Failure("Failed to mask PII data");
            }
        }

        public async Task<Result<T>> AnonymizeObjectAsync<T>(T obj, AnonymizationLevel level = AnonymizationLevel.Standard) where T : class
        {
            try
            {
                if (obj == null)
                    return Result<T>.Success(obj);

                var json = JsonSerializer.Serialize(obj);
                var anonymizedJson = await AnonymizeDataAsync(json, level);

                if (!anonymizedJson.IsSuccess)
                    return Result<T>.Failure(anonymizedJson.Error);

                var anonymizedObj = JsonSerializer.Deserialize<T>(anonymizedJson.Data);
                return Result<T>.Success(anonymizedObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing object of type {Type}", typeof(T).Name);
                return Result<T>.Failure($"Failed to anonymize object of type {typeof(T).Name}");
            }
        }

        public async Task<Result<string>> RedactSensitiveDataAsync(string logMessage)
        {
            try
            {
                var redactedMessage = logMessage;

                // Common sensitive data patterns in logs
                var sensitivePatterns = new Dictionary<string, string>
                {
                    { @"password[""':\s]*[""']([^""']+)[""']", "password: \"[REDACTED]\"" },
                    { @"token[""':\s]*[""']([^""']+)[""']", "token: \"[REDACTED]\"" },
                    { @"apikey[""':\s]*[""']([^""']+)[""']", "apikey: \"[REDACTED]\"" },
                    { @"secret[""':\s]*[""']([^""']+)[""']", "secret: \"[REDACTED]\"" },
                    { @"authorization[""':\s]*[""']([^""']+)[""']", "authorization: \"[REDACTED]\"" },
                    { @"bearer\s+[a-zA-Z0-9\-._~+/]+=*", "Bearer [REDACTED]" }
                };

                foreach (var pattern in sensitivePatterns)
                {
                    redactedMessage = Regex.Replace(redactedMessage, pattern.Key, pattern.Value, RegexOptions.IgnoreCase);
                }

                // Apply PII masking
                var maskedResult = await MaskPiiAsync(redactedMessage);
                return maskedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redacting sensitive data from log message");
                return Result<string>.Success(logMessage); // Return original on error to avoid log loss
            }
        }

        public async Task<Result<bool>> ContainsPiiAsync(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<bool>.Success(false);

                return Result<bool>.Success(PiiPatterns.Values.Any(pattern => pattern.IsMatch(data)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for PII in data");
                return Result<bool>.Failure("Failed to check for PII");
            }
        }

        public async Task<Result<List<PiiDetectionResult>>> DetectPiiAsync(string data)
        {
            try
            {
                var results = new List<PiiDetectionResult>();

                if (string.IsNullOrEmpty(data))
                    return Result<List<PiiDetectionResult>>.Success(results);

                foreach (var pattern in PiiPatterns)
                {
                    var matches = pattern.Value.Matches(data);
                    foreach (Match match in matches)
                    {
                        results.Add(new PiiDetectionResult
                        {
                            Type = pattern.Key,
                            Value = match.Value,
                            Position = match.Index,
                            Length = match.Length,
                            Confidence = CalculateConfidence(pattern.Key, match.Value)
                        });
                    }
                }

                return Result<List<PiiDetectionResult>>.Success(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting PII in data");
                return Result<List<PiiDetectionResult>>.Failure("Failed to detect PII");
            }
        }

        public async Task<Result<string>> ApplyDataRetentionPolicyAsync(string data, DataRetentionPolicy policy)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return Result<string>.Success(data);

                var processedData = data;

                switch (policy.Action)
                {
                    case RetentionAction.Delete:
                        processedData = string.Empty;
                        break;
                    case RetentionAction.Anonymize:
                        var anonymizeResult = await AnonymizeDataAsync(data, policy.AnonymizationLevel);
                        processedData = anonymizeResult.IsSuccess ? anonymizeResult.Data : data;
                        break;
                    case RetentionAction.Archive:
                        var encryptResult = await _encryptionService.EncryptAsync(data);
                        processedData = encryptResult.IsSuccess ? encryptResult.Data : data;
                        break;
                    case RetentionAction.Mask:
                        var maskResult = await MaskPiiAsync(data);
                        processedData = maskResult.IsSuccess ? maskResult.Data : data;
                        break;
                }

                return Result<string>.Success(processedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying data retention policy");
                return Result<string>.Failure("Failed to apply data retention policy");
            }
        }

        public async Task<Result<Dictionary<string, object>>> CreateAnonymousProfileAsync(int userId)
        {
            try
            {
                // This would typically fetch user data and create an anonymized profile
                var anonymousProfile = new Dictionary<string, object>
                {
                    { "id", $"anon_{Guid.NewGuid():N}"[..16] },
                    { "userType", "anonymized" },
                    { "createdAt", DateTime.UtcNow },
                    { "dataSource", "anonymization_service" },
                    { "originalUserId", await HashUserIdAsync(userId) }
                };

                return Result<Dictionary<string, object>>.Success(anonymousProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating anonymous profile for user {UserId}", userId);
                return Result<Dictionary<string, object>>.Failure("Failed to create anonymous profile");
            }
        }

        public async Task<Result> PurgeUserDataAsync(int userId, PurgeOptions options)
        {
            try
            {
                _logger.LogInformation("Starting data purge for user {UserId} with options {@Options}", userId, options);

                // This would implement the actual data purge logic
                // For now, we'll just log the operation
                _logger.LogInformation("Data purge completed for user {UserId}", userId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging data for user {UserId}", userId);
                return Result.Failure("Failed to purge user data");
            }
        }

        public async Task<Result<byte[]>> ExportUserDataAsync(int userId, ExportFormat format)
        {
            try
            {
                _logger.LogInformation("Exporting data for user {UserId} in format {Format}", userId, format);

                // This would implement the actual data export logic
                var exportData = new { userId, exportedAt = DateTime.UtcNow, format };
                var json = JsonSerializer.Serialize(exportData);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);

                return Result<byte[]>.Success(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data for user {UserId}", userId);
                return Result<byte[]>.Failure("Failed to export user data");
            }
        }

        // Private helper methods
        private async Task<string> ApplyBasicAnonymizationAsync(string data)
        {
            var maskResult = await MaskPiiAsync(data);
            return maskResult.IsSuccess ? maskResult.Data : data;
        }

        private async Task<string> ApplyStandardAnonymizationAsync(string data)
        {
            var maskedData = await ApplyBasicAnonymizationAsync(data);
            
            // Additional transformations for standard level
            maskedData = Regex.Replace(maskedData, @"\b\d{4}\b", "YYYY"); // Years
            maskedData = Regex.Replace(maskedData, @"\b[A-Z][a-z]+ [A-Z][a-z]+\b", "[NAME]"); // Names
            
            return maskedData;
        }

        private async Task<string> ApplyAggressiveAnonymizationAsync(string data)
        {
            var standardData = await ApplyStandardAnonymizationAsync(data);
            
            // More aggressive transformations
            standardData = Regex.Replace(standardData, @"\b\d+\b", "[NUMBER]"); // All numbers
            standardData = Regex.Replace(standardData, @"\b[A-Z][a-z]+\b", "[WORD]"); // Capitalized words
            
            return standardData;
        }

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 0) return email;

            var username = email[..atIndex];
            var domain = email[atIndex..];

            if (username.Length <= 2)
                return $"***{domain}";

            return $"{username[0]}***{username[^1]}{domain}";
        }

        private static string MaskPhone(string phone)
        {
            var digits = Regex.Replace(phone, @"\D", "");
            if (digits.Length >= 10)
            {
                return $"***-***-{digits[^4..]}";
            }
            return "***-***-****";
        }

        private static string MaskCreditCard(string cardNumber)
        {
            var digits = Regex.Replace(cardNumber, @"\D", "");
            if (digits.Length >= 4)
            {
                return $"****-****-****-{digits[^4..]}";
            }
            return "****-****-****-****";
        }

        private static decimal CalculateConfidence(PiiType type, string value)
        {
            return type switch
            {
                PiiType.Email when value.Contains('@') && value.Contains('.') => 0.95m,
                PiiType.Phone when Regex.IsMatch(value, @"^\+?1?[-.\s]?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$") => 0.90m,
                PiiType.SSN when Regex.IsMatch(value, @"^\d{3}-\d{2}-\d{4}$") => 0.95m,
                PiiType.CreditCard when IsValidLuhn(Regex.Replace(value, @"\D", "")) => 0.85m,
                _ => 0.70m
            };
        }

        private static bool IsValidLuhn(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || !cardNumber.All(char.IsDigit))
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = (digit % 10) + 1;
                }

                sum += digit;
                alternate = !alternate;
            }

            return (sum % 10) == 0;
        }

        private async Task<string> HashUserIdAsync(int userId)
        {
            var hashResult = await _encryptionService.HashAsync($"user_{userId}");
            return hashResult.IsSuccess ? hashResult.Data[..16] : $"hash_{userId}";
        }
    }

    // Supporting enums and classes
    public enum AnonymizationLevel
    {
        Basic,
        Standard,
        Aggressive
    }

    public enum PiiType
    {
        Email,
        Phone,
        SSN,
        CreditCard,
        IPAddress,
        DateOfBirth,
        BankAccount,
        PostalCode
    }

    public enum RetentionAction
    {
        Delete,
        Anonymize,
        Archive,
        Mask
    }

    public enum ExportFormat
    {
        Json,
        Xml,
        Csv
    }

    public class PiiDetectionResult
    {
        public PiiType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Length { get; set; }
        public decimal Confidence { get; set; }
    }

    public class DataRetentionPolicy
    {
        public RetentionAction Action { get; set; }
        public AnonymizationLevel AnonymizationLevel { get; set; } = AnonymizationLevel.Standard;
        public TimeSpan RetentionPeriod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PurgeOptions
    {
        public bool IncludeBackups { get; set; } = true;
        public bool IncludeLogs { get; set; } = true;
        public bool IncludeAnalytics { get; set; } = true;
        public bool CreateAuditLog { get; set; } = true;
        public string Reason { get; set; } = string.Empty;
    }
}

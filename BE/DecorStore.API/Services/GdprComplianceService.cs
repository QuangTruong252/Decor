using DecorStore.API.Common;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static DecorStore.API.Services.DataAnonymizationService;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for GDPR compliance and data privacy management
    /// </summary>
    public interface IGdprComplianceService
    {
        Task<Result> RecordConsentAsync(int userId, ConsentRequest consent);
        Task<Result> WithdrawConsentAsync(int userId, string consentType, string reason);
        Task<Result<UserConsent>> GetUserConsentAsync(int userId);
        Task<Result<bool>> HasValidConsentAsync(int userId, string consentType);
        Task<Result<PersonalDataExport>> ExportPersonalDataAsync(int userId);
        Task<Result> RequestDataDeletionAsync(int userId, DataDeletionRequest request);
        Task<Result> ProcessDataDeletionAsync(int userId, DeletionOptions options);
        Task<Result> RectifyPersonalDataAsync(int userId, DataRectificationRequest request);
        Task<Result<DataProcessingRecord>> GetDataProcessingRecordAsync(int userId);
        Task<Result<List<DataRetentionRecord>>> GetDataRetentionRecordsAsync(int userId);
        Task<Result> ProcessDataPortabilityRequestAsync(int userId, DataPortabilityRequest request);
        Task<Result<List<ConsentHistory>>> GetConsentHistoryAsync(int userId);
        Task<Result> ScheduleDataRetentionReviewAsync(int userId, DateTime reviewDate);
        Task<Result<GdprComplianceReport>> GenerateComplianceReportAsync(DateTime from, DateTime to);
        Task<Result> HandleDataBreachNotificationAsync(DataBreachNotification notification);
    }

    public class GdprComplianceService : IGdprComplianceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GdprComplianceService> _logger;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly IDataAnonymizationService _anonymizationService;
        private readonly IDataEncryptionService _encryptionService;

        public GdprComplianceService(
            IUnitOfWork unitOfWork,
            ILogger<GdprComplianceService> logger,
            ISecurityEventLogger securityLogger,
            IDataAnonymizationService anonymizationService,
            IDataEncryptionService encryptionService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _securityLogger = securityLogger;
            _anonymizationService = anonymizationService;
            _encryptionService = encryptionService;
        }

        public async Task<Result> RecordConsentAsync(int userId, ConsentRequest consent)
        {
            try
            {
                _logger.LogInformation("Recording consent for user {UserId}, type: {ConsentType}", userId, consent.ConsentType);

                var consentRecord = new UserConsentRecord
                {
                    UserId = userId,
                    ConsentType = consent.ConsentType,
                    IsGranted = consent.IsGranted,
                    Purpose = consent.Purpose,
                    LegalBasis = consent.LegalBasis,
                    ConsentTimestamp = DateTime.UtcNow,
                    ExpiryDate = consent.ExpiryDate,
                    ConsentVersion = consent.ConsentVersion,
                    ConsentText = consent.ConsentText,
                    IpAddress = consent.IpAddress,
                    UserAgent = consent.UserAgent,
                    ConsentMethod = consent.ConsentMethod
                };

                // Store consent record (this would use a repository)
                await StoreConsentRecordAsync(consentRecord);                // Log the consent event
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "consent", "record", 
                    consent.IpAddress, true);

                // Check if this affects data processing
                if (!consent.IsGranted && IsEssentialConsent(consent.ConsentType))
                {
                    await HandleEssentialConsentWithdrawalAsync(userId, consent.ConsentType);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording consent for user {UserId}", userId);
                return Result.Failure("Failed to record consent");
            }
        }

        public async Task<Result> WithdrawConsentAsync(int userId, string consentType, string reason)
        {
            try
            {
                _logger.LogInformation("Withdrawing consent for user {UserId}, type: {ConsentType}, reason: {Reason}", 
                    userId, consentType, reason);

                // Update consent record
                var withdrawalRecord = new ConsentWithdrawal
                {
                    UserId = userId,
                    ConsentType = consentType,
                    WithdrawalTimestamp = DateTime.UtcNow,
                    Reason = reason,
                    Status = WithdrawalStatus.Processed
                };                await StoreConsentWithdrawalAsync(withdrawalRecord);                // Log the withdrawal
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "consent", "withdraw", 
                    "system", true);

                // Handle data processing implications
                if (IsEssentialConsent(consentType))
                {
                    await HandleEssentialConsentWithdrawalAsync(userId, consentType);
                }
                else
                {
                    await StopNonEssentialDataProcessingAsync(userId, consentType);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing consent for user {UserId}", userId);
                return Result.Failure("Failed to withdraw consent");
            }
        }

        public async Task<Result<UserConsent>> GetUserConsentAsync(int userId)
        {
            try
            {
                var consentRecords = await GetConsentRecordsAsync(userId);
                
                var userConsent = new UserConsent
                {
                    UserId = userId,
                    Consents = consentRecords.GroupBy(c => c.ConsentType)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.ConsentTimestamp).First()),
                    LastUpdated = consentRecords.Any() ? consentRecords.Max(c => c.ConsentTimestamp) : DateTime.MinValue
                };

                return Result<UserConsent>.Success(userConsent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user consent for user {UserId}", userId);
                return Result<UserConsent>.Failure("Failed to get user consent");
            }
        }

        public async Task<Result<bool>> HasValidConsentAsync(int userId, string consentType)
        {
            try
            {
                var consentRecords = await GetConsentRecordsAsync(userId);
                var latestConsent = consentRecords
                    .Where(c => c.ConsentType == consentType)
                    .OrderByDescending(c => c.ConsentTimestamp)
                    .FirstOrDefault();

                if (latestConsent == null)
                    return Result<bool>.Success(false);

                // Check if consent is granted and not expired
                var isValid = latestConsent.IsGranted && 
                             (latestConsent.ExpiryDate == null || latestConsent.ExpiryDate > DateTime.UtcNow);

                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking consent validity for user {UserId}", userId);
                return Result<bool>.Failure("Failed to check consent validity");
            }
        }

        public async Task<Result<PersonalDataExport>> ExportPersonalDataAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Exporting personal data for user {UserId}", userId);

                var personalData = await CollectPersonalDataAsync(userId);
                var export = new PersonalDataExport
                {
                    UserId = userId,
                    ExportDate = DateTime.UtcNow,
                    ExportId = Guid.NewGuid().ToString(),
                    DataCategories = personalData,
                    Format = ExportFormat.Json,
                    Status = ExportStatus.Completed
                };                // Log the export request
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "personal_data", "export", 
                    "system", true);

                return Result<PersonalDataExport>.Success(export);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting personal data for user {UserId}", userId);
                return Result<PersonalDataExport>.Failure("Failed to export personal data");
            }
        }

        public async Task<Result> RequestDataDeletionAsync(int userId, DataDeletionRequest request)
        {
            try
            {
                _logger.LogInformation("Processing data deletion request for user {UserId}", userId);

                var deletionRequest = new DataDeletionRecord
                {
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    RequestReason = request.Reason,
                    RequestType = request.DeletionType,
                    Status = DeletionStatus.Pending,
                    RequestedBy = request.RequestedBy,
                    ScheduledDeletionDate = request.ScheduledDate ?? DateTime.UtcNow.AddDays(30)
                };

                await StoreDeletionRequestAsync(deletionRequest);                // Log the deletion request
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "personal_data", "deletion_request", 
                    "system", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data deletion request for user {UserId}", userId);
                return Result.Failure("Failed to process data deletion request");
            }
        }

        public async Task<Result> ProcessDataDeletionAsync(int userId, DeletionOptions options)
        {
            try
            {
                _logger.LogWarning("Processing data deletion for user {UserId} with options {@Options}", userId, options);

                var deletionSummary = new DataDeletionSummary
                {
                    UserId = userId,
                    DeletionDate = DateTime.UtcNow,
                    DeletedDataTypes = new List<string>()
                };

                // Process different types of data based on options
                if (options.DeletePersonalData)
                {
                    await DeletePersonalDataAsync(userId);
                    deletionSummary.DeletedDataTypes.Add("PersonalData");
                }

                if (options.DeleteTransactionHistory)
                {
                    await AnonymizeTransactionHistoryAsync(userId);
                    deletionSummary.DeletedDataTypes.Add("TransactionHistory");
                }

                if (options.DeleteActivityLogs)
                {
                    await DeleteActivityLogsAsync(userId);
                    deletionSummary.DeletedDataTypes.Add("ActivityLogs");
                }

                if (options.DeleteBackups)
                {
                    await ScheduleBackupDeletionAsync(userId);
                    deletionSummary.DeletedDataTypes.Add("Backups");
                }

                // Store deletion summary
                await StoreDeletionSummaryAsync(deletionSummary);                // Log the deletion completion
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "personal_data", "deletion_completed", 
                    "system", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data deletion for user {UserId}", userId);
                return Result.Failure("Failed to process data deletion");
            }
        }

        public async Task<Result> RectifyPersonalDataAsync(int userId, DataRectificationRequest request)
        {
            try
            {
                _logger.LogInformation("Processing data rectification for user {UserId}", userId);

                var rectificationRecord = new DataRectificationRecord
                {
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    DataField = request.DataField,
                    OldValue = await GetCurrentDataValueAsync(userId, request.DataField),
                    NewValue = request.NewValue,
                    Reason = request.Reason,
                    Status = RectificationStatus.Completed,
                    ProcessedDate = DateTime.UtcNow
                };

                // Update the actual data
                await UpdatePersonalDataFieldAsync(userId, request.DataField, request.NewValue);

                // Store rectification record
                await StoreRectificationRecordAsync(rectificationRecord);                // Log the rectification
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "personal_data", "rectification", 
                    "system", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data rectification for user {UserId}", userId);
                return Result.Failure("Failed to process data rectification");
            }
        }

        public async Task<Result<DataProcessingRecord>> GetDataProcessingRecordAsync(int userId)
        {
            try
            {
                var processingRecord = new DataProcessingRecord
                {
                    UserId = userId,
                    ProcessingActivities = await GetProcessingActivitiesAsync(userId),
                    LegalBases = await GetLegalBasesAsync(userId),
                    DataCategories = await GetDataCategoriesAsync(userId),
                    ProcessingPurposes = await GetProcessingPurposesAsync(userId),
                    DataRecipients = await GetDataRecipientsAsync(userId),
                    RetentionPeriods = await GetRetentionPeriodsAsync(userId),
                    LastUpdated = DateTime.UtcNow
                };

                return Result<DataProcessingRecord>.Success(processingRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data processing record for user {UserId}", userId);
                return Result<DataProcessingRecord>.Failure("Failed to get data processing record");
            }
        }

        public async Task<Result<List<DataRetentionRecord>>> GetDataRetentionRecordsAsync(int userId)
        {
            try
            {
                var retentionRecords = await GetUserDataRetentionRecordsAsync(userId);
                return Result<List<DataRetentionRecord>>.Success(retentionRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data retention records for user {UserId}", userId);
                return Result<List<DataRetentionRecord>>.Failure("Failed to get data retention records");
            }
        }

        public async Task<Result> ProcessDataPortabilityRequestAsync(int userId, DataPortabilityRequest request)
        {
            try
            {
                _logger.LogInformation("Processing data portability request for user {UserId}", userId);

                var portabilityData = await ExportPortableDataAsync(userId, request.DataTypes);
                
                var portabilityRecord = new DataPortabilityRecord
                {
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    DataTypes = request.DataTypes,
                    Format = request.Format,
                    DeliveryMethod = request.DeliveryMethod,
                    Status = PortabilityStatus.Completed,
                    CompletedDate = DateTime.UtcNow,
                    ExportLocation = await StorePortableDataAsync(portabilityData, request.Format)
                };

                await StorePortabilityRecordAsync(portabilityRecord);                // Log the portability request
                await _securityLogger.LogDataAccessAsync(userId.ToString(), "personal_data", "portability", 
                    "system", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data portability request for user {UserId}", userId);
                return Result.Failure("Failed to process data portability request");
            }
        }

        public async Task<Result<List<ConsentHistory>>> GetConsentHistoryAsync(int userId)
        {
            try
            {
                var consentRecords = await GetConsentRecordsAsync(userId);
                var withdrawalRecords = await GetConsentWithdrawalsAsync(userId);

                var history = new List<ConsentHistory>();

                // Add consent grants
                history.AddRange(consentRecords.Select(c => new ConsentHistory
                {
                    Timestamp = c.ConsentTimestamp,
                    Action = ConsentAction.Granted,
                    ConsentType = c.ConsentType,
                    Details = $"Consent granted for {c.Purpose}"
                }));

                // Add consent withdrawals
                history.AddRange(withdrawalRecords.Select(w => new ConsentHistory
                {
                    Timestamp = w.WithdrawalTimestamp,
                    Action = ConsentAction.Withdrawn,
                    ConsentType = w.ConsentType,
                    Details = $"Consent withdrawn: {w.Reason}"
                }));

                return Result<List<ConsentHistory>>.Success(history.OrderByDescending(h => h.Timestamp).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consent history for user {UserId}", userId);
                return Result<List<ConsentHistory>>.Failure("Failed to get consent history");
            }
        }

        public async Task<Result> ScheduleDataRetentionReviewAsync(int userId, DateTime reviewDate)
        {
            try
            {
                var review = new DataRetentionReview
                {
                    UserId = userId,
                    ScheduledDate = reviewDate,
                    Status = ReviewStatus.Scheduled,
                    CreatedDate = DateTime.UtcNow
                };

                await StoreRetentionReviewAsync(review);

                _logger.LogInformation("Scheduled data retention review for user {UserId} on {ReviewDate}", userId, reviewDate);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling data retention review for user {UserId}", userId);
                return Result.Failure("Failed to schedule data retention review");
            }
        }

        public async Task<Result<GdprComplianceReport>> GenerateComplianceReportAsync(DateTime from, DateTime to)
        {
            try
            {
                _logger.LogInformation("Generating GDPR compliance report from {From} to {To}", from, to);

                var report = new GdprComplianceReport
                {
                    ReportId = Guid.NewGuid().ToString(),
                    Period = new DateRange { From = from, To = to },
                    GeneratedAt = DateTime.UtcNow,
                    ConsentMetrics = await CalculateConsentMetricsAsync(from, to),
                    DataSubjectRights = await CalculateDataSubjectRightsMetricsAsync(from, to),
                    DataBreaches = await GetDataBreachesAsync(from, to),
                    ComplianceScore = await CalculateComplianceScoreAsync(from, to),
                    Recommendations = await GenerateComplianceRecommendationsAsync()
                };

                return Result<GdprComplianceReport>.Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating GDPR compliance report");
                return Result<GdprComplianceReport>.Failure("Failed to generate GDPR compliance report");
            }
        }

        public async Task<Result> HandleDataBreachNotificationAsync(DataBreachNotification notification)
        {
            try
            {
                _logger.LogWarning("Handling data breach notification: {BreachType}", notification.BreachType);

                var breachRecord = new DataBreachRecord
                {
                    BreachId = Guid.NewGuid().ToString(),
                    BreachType = notification.BreachType,
                    DiscoveryDate = notification.DiscoveryDate,
                    Description = notification.Description,
                    AffectedUsers = notification.AffectedUsers,
                    DataCategories = notification.DataCategories,
                    Severity = notification.Severity,
                    Status = BreachStatus.UnderInvestigation,
                    ReportedToAuthority = false,
                    NotificationRequired = notification.Severity >= BreachSeverity.High
                };

                await StoreDataBreachRecordAsync(breachRecord);

                // Log security incident
                await _securityLogger.LogSecurityViolationAsync("DataBreach", null, "system", 
                    $"Data breach reported: {notification.BreachType}", GetBreachRiskScore(notification.Severity));

                // If high severity, start breach response process
                if (notification.Severity >= BreachSeverity.High)
                {
                    await InitiateBreachResponseAsync(breachRecord);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling data breach notification");
                return Result.Failure("Failed to handle data breach notification");
            }
        }

        // Private helper methods (placeholder implementations)
        private async Task<List<UserConsentRecord>> GetConsentRecordsAsync(int userId)
        {
            // This would query the database for consent records
            return new List<UserConsentRecord>();
        }

        private async Task StoreConsentRecordAsync(UserConsentRecord record)
        {
            // Store in database
            await Task.Delay(1);
        }

        private async Task StoreConsentWithdrawalAsync(ConsentWithdrawal withdrawal)
        {
            // Store in database
            await Task.Delay(1);
        }

        private async Task<Dictionary<string, object>> CollectPersonalDataAsync(int userId)
        {
            // Collect all personal data for the user
            return new Dictionary<string, object>();
        }

        private static bool IsEssentialConsent(string consentType)
        {
            var essentialTypes = new[] { "service_provision", "legal_compliance", "legitimate_interest" };
            return essentialTypes.Contains(consentType);
        }

        private async Task HandleEssentialConsentWithdrawalAsync(int userId, string consentType)
        {
            // Handle withdrawal of essential consent (may require account closure)
            await Task.Delay(1);
        }

        private async Task StopNonEssentialDataProcessingAsync(int userId, string consentType)
        {
            // Stop processing for non-essential purposes
            await Task.Delay(1);
        }

        private async Task StoreDeletionRequestAsync(DataDeletionRecord record)
        {
            // Store deletion request in database
            await Task.Delay(1);
        }

        private async Task DeletePersonalDataAsync(int userId)
        {
            // Delete or anonymize personal data
            await Task.Delay(1);
        }

        private async Task AnonymizeTransactionHistoryAsync(int userId)
        {
            // Anonymize transaction history while preserving business data
            await Task.Delay(1);
        }

        private async Task DeleteActivityLogsAsync(int userId)
        {
            // Delete user activity logs
            await Task.Delay(1);
        }

        private async Task ScheduleBackupDeletionAsync(int userId)
        {
            // Schedule deletion from backups
            await Task.Delay(1);
        }

        private async Task StoreDeletionSummaryAsync(DataDeletionSummary summary)
        {
            // Store deletion summary
            await Task.Delay(1);
        }

        private async Task<string> GetCurrentDataValueAsync(int userId, string dataField)
        {
            // Get current value of data field
            return string.Empty;
        }

        private async Task UpdatePersonalDataFieldAsync(int userId, string dataField, string newValue)
        {
            // Update personal data field
            await Task.Delay(1);
        }

        private async Task StoreRectificationRecordAsync(DataRectificationRecord record)
        {
            // Store rectification record
            await Task.Delay(1);
        }

        private async Task<List<string>> GetProcessingActivitiesAsync(int userId)
        {
            return new List<string>();
        }

        private async Task<List<string>> GetLegalBasesAsync(int userId)
        {
            return new List<string>();
        }

        private async Task<List<string>> GetDataCategoriesAsync(int userId)
        {
            return new List<string>();
        }

        private async Task<List<string>> GetProcessingPurposesAsync(int userId)
        {
            return new List<string>();
        }

        private async Task<List<string>> GetDataRecipientsAsync(int userId)
        {
            return new List<string>();
        }

        private async Task<Dictionary<string, TimeSpan>> GetRetentionPeriodsAsync(int userId)
        {
            return new Dictionary<string, TimeSpan>();
        }

        private async Task<List<DataRetentionRecord>> GetUserDataRetentionRecordsAsync(int userId)
        {
            return new List<DataRetentionRecord>();
        }

        private async Task<Dictionary<string, object>> ExportPortableDataAsync(int userId, List<string> dataTypes)
        {
            return new Dictionary<string, object>();
        }

        private async Task<string> StorePortableDataAsync(Dictionary<string, object> data, ExportFormat format)
        {
            return $"export_{Guid.NewGuid():N}";
        }

        private async Task StorePortabilityRecordAsync(DataPortabilityRecord record)
        {
            await Task.Delay(1);
        }

        private async Task<List<ConsentWithdrawal>> GetConsentWithdrawalsAsync(int userId)
        {
            return new List<ConsentWithdrawal>();
        }

        private async Task StoreRetentionReviewAsync(DataRetentionReview review)
        {
            await Task.Delay(1);
        }

        private async Task<ConsentMetrics> CalculateConsentMetricsAsync(DateTime from, DateTime to)
        {
            return new ConsentMetrics();
        }

        private async Task<DataSubjectRightsMetrics> CalculateDataSubjectRightsMetricsAsync(DateTime from, DateTime to)
        {
            return new DataSubjectRightsMetrics();
        }

        private async Task<List<DataBreachRecord>> GetDataBreachesAsync(DateTime from, DateTime to)
        {
            return new List<DataBreachRecord>();
        }

        private async Task<decimal> CalculateComplianceScoreAsync(DateTime from, DateTime to)
        {
            return 95.0m; // Placeholder
        }

        private async Task<List<string>> GenerateComplianceRecommendationsAsync()
        {
            return new List<string> { "Continue regular compliance monitoring" };
        }

        private async Task StoreDataBreachRecordAsync(DataBreachRecord record)
        {
            await Task.Delay(1);
        }

        private async Task InitiateBreachResponseAsync(DataBreachRecord breach)
        {
            // Initiate breach response procedures
            await Task.Delay(1);
        }

        private static decimal GetBreachRiskScore(BreachSeverity severity)
        {
            return severity switch
            {
                BreachSeverity.Low => 3.0m,
                BreachSeverity.Medium => 6.0m,
                BreachSeverity.High => 8.5m,
                BreachSeverity.Critical => 10.0m,
                _ => 5.0m
            };
        }
    }

    // Supporting classes and enums
    public class ConsentRequest
    {
        public string ConsentType { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string ConsentVersion { get; set; } = string.Empty;
        public string ConsentText { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public ConsentMethod ConsentMethod { get; set; }
    }

    public class UserConsentRecord
    {
        public int UserId { get; set; }
        public string ConsentType { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public DateTime ConsentTimestamp { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ConsentVersion { get; set; } = string.Empty;
        public string ConsentText { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public ConsentMethod ConsentMethod { get; set; }
    }

    public class UserConsent
    {
        public int UserId { get; set; }
        public Dictionary<string, UserConsentRecord> Consents { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class ConsentWithdrawal
    {
        public int UserId { get; set; }
        public string ConsentType { get; set; } = string.Empty;
        public DateTime WithdrawalTimestamp { get; set; }
        public string Reason { get; set; } = string.Empty;
        public WithdrawalStatus Status { get; set; }
    }

    public class DataDeletionRequest
    {
        public DeletionType DeletionType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
    }

    public class DataDeletionRecord
    {
        public int UserId { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestReason { get; set; } = string.Empty;
        public DeletionType RequestType { get; set; }
        public DeletionStatus Status { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime ScheduledDeletionDate { get; set; }
    }

    public class DeletionOptions
    {
        public bool DeletePersonalData { get; set; } = true;
        public bool DeleteTransactionHistory { get; set; } = false;
        public bool DeleteActivityLogs { get; set; } = true;
        public bool DeleteBackups { get; set; } = true;
        public bool PreserveAuditTrail { get; set; } = true;
    }

    public class DataDeletionSummary
    {
        public int UserId { get; set; }
        public DateTime DeletionDate { get; set; }
        public List<string> DeletedDataTypes { get; set; } = new();
    }

    public class DataRectificationRequest
    {
        public string DataField { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class DataRectificationRecord
    {
        public int UserId { get; set; }
        public DateTime RequestDate { get; set; }
        public string DataField { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public RectificationStatus Status { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }

    public class PersonalDataExport
    {
        public int UserId { get; set; }
        public DateTime ExportDate { get; set; }
        public string ExportId { get; set; } = string.Empty;
        public Dictionary<string, object> DataCategories { get; set; } = new();
        public ExportFormat Format { get; set; }
        public ExportStatus Status { get; set; }
    }

    public class DataProcessingRecord
    {
        public int UserId { get; set; }
        public List<string> ProcessingActivities { get; set; } = new();
        public List<string> LegalBases { get; set; } = new();
        public List<string> DataCategories { get; set; } = new();
        public List<string> ProcessingPurposes { get; set; } = new();
        public List<string> DataRecipients { get; set; } = new();
        public Dictionary<string, TimeSpan> RetentionPeriods { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class DataRetentionRecord
    {
        public string DataType { get; set; } = string.Empty;
        public TimeSpan RetentionPeriod { get; set; }
        public DateTime LastReviewDate { get; set; }
        public DateTime NextReviewDate { get; set; }
        public RetentionStatus Status { get; set; }
    }

    public class DataPortabilityRequest
    {
        public List<string> DataTypes { get; set; } = new();
        public ExportFormat Format { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
    }

    public class DataPortabilityRecord
    {
        public int UserId { get; set; }
        public DateTime RequestDate { get; set; }
        public List<string> DataTypes { get; set; } = new();
        public ExportFormat Format { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public PortabilityStatus Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string ExportLocation { get; set; } = string.Empty;
    }

    public class ConsentHistory
    {
        public DateTime Timestamp { get; set; }
        public ConsentAction Action { get; set; }
        public string ConsentType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class DataRetentionReview
    {
        public int UserId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public ReviewStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GdprComplianceReport
    {
        public string ReportId { get; set; } = string.Empty;
        public DateRange Period { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public ConsentMetrics ConsentMetrics { get; set; } = new();
        public DataSubjectRightsMetrics DataSubjectRights { get; set; } = new();
        public List<DataBreachRecord> DataBreaches { get; set; } = new();
        public decimal ComplianceScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class ConsentMetrics
    {
        public int TotalConsentRequests { get; set; }
        public int GrantedConsents { get; set; }
        public int DeniedConsents { get; set; }
        public int WithdrawnConsents { get; set; }
        public int ExpiredConsents { get; set; }
    }

    public class DataSubjectRightsMetrics
    {
        public int AccessRequests { get; set; }
        public int RectificationRequests { get; set; }
        public int DeletionRequests { get; set; }
        public int PortabilityRequests { get; set; }
        public int ObjectionRequests { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
    }

    public class DataBreachNotification
    {
        public string BreachType { get; set; } = string.Empty;
        public DateTime DiscoveryDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<int> AffectedUsers { get; set; } = new();
        public List<string> DataCategories { get; set; } = new();
        public BreachSeverity Severity { get; set; }
    }

    public class DataBreachRecord
    {
        public string BreachId { get; set; } = string.Empty;
        public string BreachType { get; set; } = string.Empty;
        public DateTime DiscoveryDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<int> AffectedUsers { get; set; } = new();
        public List<string> DataCategories { get; set; } = new();
        public BreachSeverity Severity { get; set; }
        public BreachStatus Status { get; set; }
        public bool ReportedToAuthority { get; set; }
        public bool NotificationRequired { get; set; }
    }

    // Enums
    public enum ConsentMethod { WebForm, Email, Phone, InPerson, Api }
    public enum WithdrawalStatus { Pending, Processed, Failed }
    public enum DeletionType { Complete, Partial, Anonymization }
    public enum DeletionStatus { Pending, InProgress, Completed, Failed }    public enum RectificationStatus { Pending, InProgress, Completed, Failed }
    public enum ExportStatus { Pending, InProgress, Completed, Failed }
    public enum PortabilityStatus { Pending, InProgress, Completed, Failed }
    public enum DeliveryMethod { Download, Email, ApiEndpoint }
    public enum ConsentAction { Granted, Withdrawn, Updated, Expired }
    public enum ReviewStatus { Scheduled, InProgress, Completed, Overdue }
    public enum RetentionStatus { Active, Expired, UnderReview, Archived }
    public enum BreachSeverity { Low, Medium, High, Critical }
    public enum BreachStatus { Discovered, UnderInvestigation, Contained, Resolved }
}

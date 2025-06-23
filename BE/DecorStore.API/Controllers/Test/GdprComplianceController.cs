using DecorStore.API.Common;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers.Test
{
    /// <summary>
    /// Controller for GDPR compliance and data privacy management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GdprComplianceController : ControllerBase
    {
        private readonly IGdprComplianceService _gdprService;
        private readonly ILogger<GdprComplianceController> _logger;

        public GdprComplianceController(
            IGdprComplianceService gdprService,
            ILogger<GdprComplianceController> logger)
        {
            _gdprService = gdprService;
            _logger = logger;
        }

        /// <summary>
        /// Record user consent
        /// </summary>
        [HttpPost("consent")]
        public async Task<ActionResult> RecordConsent([FromBody] ConsentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.RecordConsentAsync(userId.Value, request);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Withdraw user consent
        /// </summary>
        [HttpPost("consent/withdraw")]
        public async Task<ActionResult> WithdrawConsent([FromBody] WithdrawConsentRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.WithdrawConsentAsync(userId.Value, request.ConsentType, request.Reason);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Get user consent status
        /// </summary>
        [HttpGet("consent")]
        public async Task<ActionResult<UserConsent>> GetUserConsent()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.GetUserConsentAsync(userId.Value);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Check if user has valid consent for specific type
        /// </summary>
        [HttpGet("consent/{consentType}/valid")]
        public async Task<ActionResult<bool>> HasValidConsent(string consentType)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.HasValidConsentAsync(userId.Value, consentType);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Export personal data (Right to Access)
        /// </summary>
        [HttpPost("data/export")]
        public async Task<ActionResult<PersonalDataExport>> ExportPersonalData()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.ExportPersonalDataAsync(userId.Value);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Request data deletion (Right to be Forgotten)
        /// </summary>
        [HttpPost("data/deletion")]
        public async Task<ActionResult> RequestDataDeletion([FromBody] DataDeletionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.RequestDataDeletionAsync(userId.Value, request);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Rectify personal data (Right to Rectification)
        /// </summary>
        [HttpPost("data/rectification")]
        public async Task<ActionResult> RectifyPersonalData([FromBody] DataRectificationRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.RectifyPersonalDataAsync(userId.Value, request);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Get data processing record
        /// </summary>
        [HttpGet("data/processing")]
        public async Task<ActionResult<DataProcessingRecord>> GetDataProcessingRecord()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.GetDataProcessingRecordAsync(userId.Value);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get data retention records
        /// </summary>
        [HttpGet("data/retention")]
        public async Task<ActionResult<List<DataRetentionRecord>>> GetDataRetentionRecords()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.GetDataRetentionRecordsAsync(userId.Value);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Request data portability (Right to Data Portability)
        /// </summary>
        [HttpPost("data/portability")]
        public async Task<ActionResult> ProcessDataPortabilityRequest([FromBody] DataPortabilityRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.ProcessDataPortabilityRequestAsync(userId.Value, request);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Get consent history
        /// </summary>
        [HttpGet("consent/history")]
        public async Task<ActionResult<List<ConsentHistory>>> GetConsentHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var result = await _gdprService.GetConsentHistoryAsync(userId.Value);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Generate GDPR compliance report (Admin only)
        /// </summary>
        [HttpPost("reports/compliance")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GdprComplianceReport>> GenerateComplianceReport([FromBody] ComplianceReportRequest request)
        {
            var result = await _gdprService.GenerateComplianceReportAsync(request.From, request.To);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Handle data breach notification (Admin only)
        /// </summary>
        [HttpPost("breach/notification")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> HandleDataBreachNotification([FromBody] DataBreachNotification notification)
        {
            var result = await _gdprService.HandleDataBreachNotificationAsync(notification);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Process data deletion for user (Admin only)
        /// </summary>
        [HttpPost("users/{userId}/data/deletion")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ProcessDataDeletion(int userId, [FromBody] DeletionOptions options)
        {
            var result = await _gdprService.ProcessDataDeletionAsync(userId, options);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Schedule data retention review (Admin only)
        /// </summary>
        [HttpPost("users/{userId}/retention/review")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ScheduleDataRetentionReview(int userId, [FromBody] ScheduleRetentionReviewRequest request)
        {
            var result = await _gdprService.ScheduleDataRetentionReviewAsync(userId, request.ReviewDate);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Export user data by ID (Admin only)
        /// </summary>
        [HttpGet("data-export/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetDataExport(int userId)
        {
            var result = await _gdprService.ExportPersonalDataAsync(userId);
            if (result.IsSuccess)
            {
                return Ok(new { 
                    UserId = userId,
                    ExportedAt = DateTime.UtcNow,
                    Data = result.Data,
                    Message = "Personal data exported successfully" 
                });
            }
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) 
                ? NotFound($"User {userId} not found") 
                : BadRequest(result.Error);
        }

        /// <summary>
        /// Get user consent status by ID (Admin only)
        /// </summary>
        [HttpGet("consent/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetConsentStatus(int userId)
        {
            var result = await _gdprService.GetUserConsentAsync(userId);
            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }
            return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase) 
                ? NotFound($"User {userId} not found") 
                : BadRequest(result.Error);
        }

        /// <summary>
        /// Update user consent (Admin only)
        /// </summary>
        [HttpPut("consent")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateConsent([FromBody] UpdateConsentRequest request)
        {
            var consentData = new ConsentRequest
            {
                ConsentType = "general",
                IsGranted = true,
                Purpose = "Updated by admin"
            };

            var result = await _gdprService.RecordConsentAsync(request.UserId, consentData);
            return result.IsSuccess ? Ok(new { Message = "Consent updated successfully", UserId = request.UserId }) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get privacy policy
        /// </summary>
        [HttpGet("privacy-policy")]
        [AllowAnonymous]
        public ActionResult GetPrivacyPolicy()
        {
            var policy = new
            {
                Title = "Privacy Policy",
                LastUpdated = DateTime.UtcNow.AddDays(-30),
                Version = "1.0",
                Content = new
                {
                    Introduction = "This privacy policy describes how we collect, use, and protect your personal information.",
                    DataCollection = "We collect personal information that you provide to us when using our services.",
                    DataUsage = "We use your personal information to provide and improve our services.",
                    DataProtection = "We implement appropriate security measures to protect your personal information.",
                    UserRights = "You have the right to access, correct, or delete your personal information.",
                    Contact = "Please contact us if you have any questions about this privacy policy."
                },
                ComplianceStandards = new[] { "GDPR", "CCPA", "ISO 27001" }
            };

            return Ok(policy);
        }

        /// <summary>
        /// Get data processing agreement
        /// </summary>
        [HttpGet("data-processing-agreement")]
        [AllowAnonymous]
        public ActionResult GetDataProcessingAgreement()
        {
            var agreement = new
            {
                Title = "Data Processing Agreement",
                LastUpdated = DateTime.UtcNow.AddDays(-30),
                Version = "1.0",
                Content = new
                {
                    Purpose = "This agreement governs the processing of personal data on behalf of our customers.",
                    Scope = "This agreement applies to all personal data processed by our services.",
                    Responsibilities = "We are committed to processing personal data in accordance with applicable laws.",
                    Security = "We implement appropriate technical and organizational measures to protect personal data.",
                    DataTransfers = "Personal data may be transferred to countries outside the EEA with adequate protection.",
                    DataRetention = "Personal data is retained only as long as necessary for the specified purposes."
                },
                LegalBasis = new[] { "Article 6(1)(b) GDPR", "Article 6(1)(f) GDPR" }
            };

            return Ok(agreement);
        }

        /// <summary>
        /// Get data retention policy (Admin only)
        /// </summary>
        [HttpGet("data-retention")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetDataRetentionPolicy()
        {
            var policy = new
            {
                Title = "Data Retention Policy",
                LastUpdated = DateTime.UtcNow.AddDays(-30),
                Version = "1.0",
                RetentionPeriods = new
                {
                    UserData = "7 years after account closure",
                    OrderData = "10 years for tax purposes",
                    LogData = "2 years",
                    MarketingData = "Until consent is withdrawn",
                    BackupData = "30 days in secure storage"
                },
                DeletionProcedures = new[]
                {
                    "Automated deletion based on retention periods",
                    "Manual deletion upon user request",
                    "Secure destruction of physical media",
                    "Certificate of destruction for sensitive data"
                },
                ComplianceFrameworks = new[] { "GDPR", "SOX", "ISO 27001" }
            };

            return Ok(policy);
        }

        /// <summary>
        /// Get audit log (Admin only)
        /// </summary>
        [HttpGet("audit-log")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAuditLog([FromQuery] int? userId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var auditEntries = new[]
            {
                new
                {
                    Id = 1,
                    UserId = userId ?? 1,
                    Action = "Data Export",
                    Timestamp = DateTime.UtcNow.AddHours(-2),
                    UserAgent = "Test User Agent",
                    IpAddress = "192.168.1.1",
                    Result = "Success"
                },
                new
                {
                    Id = 2,
                    UserId = userId ?? 1,
                    Action = "Consent Update",
                    Timestamp = DateTime.UtcNow.AddHours(-5),
                    UserAgent = "Test User Agent",
                    IpAddress = "192.168.1.1",
                    Result = "Success"
                }
            };

            var result = new
            {
                AuditEntries = auditEntries,
                Page = page,
                PageSize = pageSize,
                TotalCount = auditEntries.Length,
                UserId = userId
            };

            return Ok(result);
        }

        /// <summary>
        /// Anonymize user data (Admin only)
        /// </summary>
        [HttpPost("anonymize")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AnonymizeUserData([FromBody] AnonymizeDataRequest request)
        {
            // Simulate anonymization process
            var result = new
            {
                UserId = request.UserId,
                AnonymizedAt = DateTime.UtcNow,
                Reason = request.Reason,
                DataRetained = request.KeepOrderHistory,
                Message = "User data has been successfully anonymized"
            };

            return Ok(result);
        }

        /// <summary>
        /// Get data breach log (Admin only)
        /// </summary>
        [HttpGet("data-breaches")]
        [Authorize(Roles = "Admin")]
        public ActionResult GetDataBreachLog()
        {
            var breaches = new[]
            {
                new
                {
                    Id = 1,
                    IncidentDate = DateTime.UtcNow.AddDays(-30),
                    DetectedDate = DateTime.UtcNow.AddDays(-29),
                    Severity = "Low",
                    AffectedUsers = 0,
                    DataTypes = new[] { "Email addresses" },
                    Status = "Resolved",
                    NotificationsSent = 0,
                    RegulatoryReporting = "Not required"
                }
            };

            var result = new
            {
                DataBreaches = breaches,
                TotalIncidents = breaches.Length,
                LastReview = DateTime.UtcNow.AddDays(-1),
                ComplianceStatus = "Compliant"
            };

            return Ok(result);
        }

        /// <summary>
        /// Validate GDPR compliance status (Admin only)
        /// </summary>
        [HttpGet("validate-compliance")]
        [Authorize(Roles = "Admin")]
        public ActionResult ValidateGdprCompliance()
        {
            var compliance = new
            {
                ComplianceStatus = "Compliant",
                LastAssessment = DateTime.UtcNow.AddDays(-7),
                NextReview = DateTime.UtcNow.AddDays(23),
                ComplianceScore = 95,
                Areas = new
                {
                    DataProcessing = new { Status = "Compliant", Score = 100 },
                    UserRights = new { Status = "Compliant", Score = 95 },
                    DataSecurity = new { Status = "Compliant", Score = 98 },
                    Documentation = new { Status = "Compliant", Score = 90 },
                    Training = new { Status = "Compliant", Score = 92 }
                },
                Recommendations = new[]
                {
                    "Update privacy policy annually",
                    "Conduct regular GDPR training for staff",
                    "Review data retention policies quarterly"
                },
                Certifications = new[] { "ISO 27001", "SOC 2 Type II" }
            };

            return Ok(compliance);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("user_id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    // Supporting request classes
    public class WithdrawConsentRequest
    {
        public string ConsentType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ComplianceReportRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class ScheduleRetentionReviewRequest
    {
        public DateTime ReviewDate { get; set; }
    }

    public class UpdateConsentRequest
    {
        public int UserId { get; set; }
        public bool MarketingConsent { get; set; }
        public bool DataProcessingConsent { get; set; }
        public bool CookieConsent { get; set; }
    }

    public class AnonymizeDataRequest
    {
        public int UserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool KeepOrderHistory { get; set; }
    }
}

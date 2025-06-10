using DecorStore.API.Common;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
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
}

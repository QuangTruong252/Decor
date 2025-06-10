using DecorStore.API.Common;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    /// <summary>
    /// Controller for security dashboard and monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SecurityDashboardController : ControllerBase
    {
        private readonly ISecurityDashboardService _securityDashboardService;
        private readonly ILogger<SecurityDashboardController> _logger;

        public SecurityDashboardController(
            ISecurityDashboardService securityDashboardService,
            ILogger<SecurityDashboardController> logger)
        {
            _securityDashboardService = securityDashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get security dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<SecurityDashboardData>> GetDashboardData(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityDashboardService.GetSecurityDashboardDataAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get active security alerts
        /// </summary>
        [HttpGet("alerts")]
        public async Task<ActionResult<List<SecurityAlert>>> GetActiveAlerts()
        {
            var result = await _securityDashboardService.GetActiveSecurityAlertsAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get security metrics
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<SecurityMetrics>> GetSecurityMetrics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityDashboardService.GetSecurityMetricsAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get threat indicators
        /// </summary>
        [HttpGet("threats")]
        public async Task<ActionResult<List<ThreatIndicator>>> GetThreatIndicators(
            [FromQuery] int limit = 100)
        {
            var result = await _securityDashboardService.GetThreatIndicatorsAsync(limit);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get authentication metrics
        /// </summary>
        [HttpGet("metrics/authentication")]
        public async Task<ActionResult<AuthenticationMetrics>> GetAuthenticationMetrics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityDashboardService.GetAuthenticationMetricsAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get API security metrics
        /// </summary>
        [HttpGet("metrics/api")]
        public async Task<ActionResult<ApiSecurityMetrics>> GetApiSecurityMetrics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityDashboardService.GetApiSecurityMetricsAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get security incidents
        /// </summary>
        [HttpGet("incidents")]
        public async Task<ActionResult<List<SecurityIncident>>> GetSecurityIncidents(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityDashboardService.GetSecurityIncidentsAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get compliance status
        /// </summary>
        [HttpGet("compliance")]
        public async Task<ActionResult<ComplianceStatus>> GetComplianceStatus()
        {
            var result = await _securityDashboardService.GetComplianceStatusAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get vulnerability assessment
        /// </summary>
        [HttpGet("vulnerabilities")]
        public async Task<ActionResult<VulnerabilityAssessment>> GetVulnerabilityAssessment()
        {
            var result = await _securityDashboardService.GetVulnerabilityAssessmentAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Get risk assessment
        /// </summary>
        [HttpGet("risk")]
        public async Task<ActionResult<RiskAssessment>> GetRiskAssessment()
        {
            var result = await _securityDashboardService.GetRiskAssessmentAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Dismiss a security alert
        /// </summary>
        [HttpPost("alerts/{alertId}/dismiss")]
        public async Task<ActionResult> DismissSecurityAlert(int alertId, [FromBody] DismissAlertRequest request)
        {
            var result = await _securityDashboardService.DismissSecurityAlertAsync(alertId, request.Reason);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Escalate a security incident
        /// </summary>
        [HttpPost("incidents/{incidentId}/escalate")]
        public async Task<ActionResult> EscalateSecurityIncident(int incidentId, [FromBody] EscalateIncidentRequest request)
        {
            var result = await _securityDashboardService.EscalateSecurityIncidentAsync(incidentId, request.Reason);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Generate security report
        /// </summary>
        [HttpPost("reports")]
        public async Task<ActionResult<SecurityReport>> GenerateSecurityReport([FromBody] SecurityReportRequest request)
        {
            var result = await _securityDashboardService.GenerateSecurityReportAsync(request);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
    }

    // Supporting request classes
    public class DismissAlertRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class EscalateIncidentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}

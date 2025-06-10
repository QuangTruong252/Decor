using DecorStore.API.Common;
using DecorStore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorStore.API.Controllers
{
    /// <summary>
    /// Controller for security testing and vulnerability assessment
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SecurityTestingController : ControllerBase
    {
        private readonly ISecurityTestingService _securityTestingService;
        private readonly ILogger<SecurityTestingController> _logger;

        public SecurityTestingController(
            ISecurityTestingService securityTestingService,
            ILogger<SecurityTestingController> logger)
        {
            _securityTestingService = securityTestingService;
            _logger = logger;
        }

        /// <summary>
        /// Perform comprehensive vulnerability assessment
        /// </summary>
        [HttpPost("vulnerability-assessment")]
        public async Task<ActionResult<VulnerabilityTestResults>> PerformVulnerabilityAssessment()
        {
            var result = await _securityTestingService.PerformVulnerabilityAssessmentAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Perform penetration testing
        /// </summary>
        [HttpPost("penetration-test")]
        public async Task<ActionResult<PenetrationTestResults>> PerformPenetrationTest([FromBody] PenetrationTestConfig config)
        {
            var result = await _securityTestingService.PerformPenetrationTestAsync(config);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test SQL injection vulnerabilities
        /// </summary>
        [HttpPost("test/sql-injection")]
        public async Task<ActionResult<SqlInjectionTestResults>> TestSqlInjectionVulnerabilities()
        {
            var result = await _securityTestingService.TestSqlInjectionVulnerabilitiesAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test XSS vulnerabilities
        /// </summary>
        [HttpPost("test/xss")]
        public async Task<ActionResult<XssTestResults>> TestXssVulnerabilities()
        {
            var result = await _securityTestingService.TestXssVulnerabilitiesAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test authentication mechanisms
        /// </summary>
        [HttpPost("test/authentication")]
        public async Task<ActionResult<AuthenticationTestResults>> TestAuthenticationMechanisms()
        {
            var result = await _securityTestingService.TestAuthenticationMechanismsAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test authorization controls
        /// </summary>
        [HttpPost("test/authorization")]
        public async Task<ActionResult<AuthorizationTestResults>> TestAuthorizationControls()
        {
            var result = await _securityTestingService.TestAuthorizationControlsAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test input validation
        /// </summary>
        [HttpPost("test/input-validation")]
        public async Task<ActionResult<InputValidationTestResults>> TestInputValidation()
        {
            var result = await _securityTestingService.TestInputValidationAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test rate limiting effectiveness
        /// </summary>
        [HttpPost("test/rate-limiting")]
        public async Task<ActionResult<RateLimitingTestResults>> TestRateLimitingEffectiveness()
        {
            var result = await _securityTestingService.TestRateLimitingEffectivenessAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Test security configuration
        /// </summary>
        [HttpPost("test/configuration")]
        public async Task<ActionResult<ConfigurationSecurityTestResults>> TestSecurityConfiguration()
        {
            var result = await _securityTestingService.TestSecurityConfigurationAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Check dependency vulnerabilities
        /// </summary>
        [HttpPost("test/dependencies")]
        public async Task<ActionResult<DependencyVulnerabilityResults>> CheckDependencyVulnerabilities()
        {
            var result = await _securityTestingService.CheckDependencyVulnerabilitiesAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Generate security test report
        /// </summary>
        [HttpPost("reports")]
        public async Task<ActionResult<SecurityTestReport>> GenerateSecurityTestReport([FromBody] SecurityTestReportRequest request)
        {
            var result = await _securityTestingService.GenerateSecurityTestReportAsync(request.From, request.To);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }

        /// <summary>
        /// Schedule automated security tests
        /// </summary>
        [HttpPost("schedule")]
        public async Task<ActionResult> ScheduleAutomatedSecurityTests([FromBody] SecurityTestSchedule schedule)
        {
            var result = await _securityTestingService.ScheduleAutomatedSecurityTestsAsync(schedule);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Get test execution history
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<SecurityTestExecution>>> GetTestExecutionHistory(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var result = await _securityTestingService.GetTestExecutionHistoryAsync(from, to);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
        }
    }

    // Supporting request classes
    public class SecurityTestReportRequest
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}

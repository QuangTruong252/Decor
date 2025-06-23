using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace DecorStore.Test
{
    /// <summary>
    /// Simple authentication handler for test environment that bypasses JWT complexities
    /// </summary>
    public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check for Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeaderValue = authHeader.ToString();
            if (!authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeaderValue.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            // Create a test user identity for any valid-looking token
            // In real tests, we'll use properly generated JWT tokens
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "truongadmin"),
                new Claim(ClaimTypes.Email, "truongadmin@gmail.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            // Set the user in the context immediately
            Context.User = principal;

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions { }
}

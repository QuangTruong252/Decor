using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DecorStore.Test
{
    /// <summary>
    /// Simple authentication middleware for test environment that sets up a test user
    /// when an Authorization header is present
    /// </summary>
    public class TestAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public TestAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check for Authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.ToString();
                if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeaderValue.Substring("Bearer ".Length).Trim();
                    if (!string.IsNullOrEmpty(token))
                    {
                        // Create a test user identity for any valid-looking token
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "1"),
                            new Claim(ClaimTypes.Name, "truongadmin"),
                            new Claim(ClaimTypes.Email, "truongadmin@gmail.com"),
                            new Claim(ClaimTypes.Role, "Admin"),
                            new Claim("UserId", "1"),
                            new Claim("Username", "truongadmin")
                        };

                        var identity = new ClaimsIdentity(claims, "Test");
                        var principal = new ClaimsPrincipal(identity);
                        context.User = principal;

                        Console.WriteLine($"[TEST-AUTH-MIDDLEWARE] User authenticated: {principal.Identity.Name}");
                        Console.WriteLine($"[TEST-AUTH-MIDDLEWARE] IsAuthenticated: {principal.Identity.IsAuthenticated}");
                        Console.WriteLine($"[TEST-AUTH-MIDDLEWARE] Claims: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");
                    }
                }
            }

            await _next(context);
        }
    }
}

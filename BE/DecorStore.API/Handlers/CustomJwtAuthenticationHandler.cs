using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using DecorStore.API.Configuration;

namespace DecorStore.API.Handlers
{
    /// <summary>
    /// Custom JWT authentication handler that ensures HttpContext.User is properly set
    /// Fixed for test environment compatibility - handles double authentication calls gracefully
    /// </summary>
    public class CustomJwtAuthenticationHandler : AuthenticationHandler<JwtBearerOptions>
    {
        private const string AuthResultKey = "CustomJwtAuthResult";
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<string, (AuthenticateResult Result, DateTime Expiry)> _globalCache = new();

        public CustomJwtAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IConfiguration configuration)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Console.WriteLine($"[CUSTOM-JWT] ==================== AUTHENTICATION START ====================");
            Console.WriteLine($"[CUSTOM-JWT] HandleAuthenticateAsync called for path: {Request.Path}");
            Console.WriteLine($"[CUSTOM-JWT] Request ID: {Context.TraceIdentifier}");
            Console.WriteLine($"[CUSTOM-JWT] Thread ID: {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"[CUSTOM-JWT] Authorization header: {Request.Headers.Authorization}");
            Console.WriteLine($"[CUSTOM-JWT] All headers: {string.Join(", ", Request.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value.ToArray())}"))}");
            Console.WriteLine($"[CUSTOM-JWT] Current User: {Context.User?.Identity?.Name} (Authenticated: {Context.User?.Identity?.IsAuthenticated})");

            // Check if we already have a successful authentication result for this request
            if (Context.Items.TryGetValue(AuthResultKey, out var existingResult) && existingResult is AuthenticateResult existing)
            {
                Console.WriteLine($"[CUSTOM-JWT] Using cached authentication result: {existing.Succeeded}");
                if (existing.Succeeded && existing.Principal != null)
                {
                    Context.User = existing.Principal;
                    Console.WriteLine($"[CUSTOM-JWT] Restored user from cache: {Context.User?.Identity?.Name}");
                }
                Console.WriteLine($"[CUSTOM-JWT] ==================== AUTHENTICATION CACHED ====================");
                return existing;
            }

            // Get the authorization header
            var authHeaderValue = Request.Headers.Authorization.ToString();

            // Create a cache key based on the token and request path
            // Use a default cache key for requests without auth headers
            var cacheKey = string.IsNullOrEmpty(authHeaderValue)
                ? $"{Request.Path}_NO_AUTH"
                : $"{Request.Path}_{authHeaderValue.GetHashCode()}";

            // Skip global cache in test environment to avoid double authentication issues
            var isTestEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Test", StringComparison.OrdinalIgnoreCase) == true;

            // Check global cache first (skip in test environment)
            if (!isTestEnvironment && _globalCache.TryGetValue(cacheKey, out var cachedEntry) && cachedEntry.Expiry > DateTime.UtcNow)
            {
                Console.WriteLine($"[CUSTOM-JWT] Using global cached authentication result: {cachedEntry.Result.Succeeded}");
                if (cachedEntry.Result.Succeeded && cachedEntry.Result.Principal != null)
                {
                    Context.User = cachedEntry.Result.Principal;
                }
                return cachedEntry.Result;
            }

            // Check if we've already processed authentication for this request
            if (Context.Items.TryGetValue(AuthResultKey, out var cachedResult) && cachedResult is AuthenticateResult cached)
            {
                Console.WriteLine($"[CUSTOM-JWT] Using context cached authentication result: {cached.Succeeded}");
                if (cached.Succeeded && cached.Principal != null)
                {
                    Context.User = cached.Principal;
                }
                return cached;
            }

            // Check if user is already authenticated in this context to avoid double authentication
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"[CUSTOM-JWT] User already authenticated: {Context.User.Identity.Name}");
                var successResult = AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name));
                Context.Items[AuthResultKey] = successResult;

                // Cache globally for a short time (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (successResult, DateTime.UtcNow.AddMinutes(1)));
                }

                return successResult;
            }

            // Get the authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                Console.WriteLine($"[CUSTOM-JWT] No Authorization header found");

                var noAuthResult = AuthenticateResult.NoResult();
                Context.Items[AuthResultKey] = noAuthResult;

                // Cache the no-auth result briefly to avoid repeated processing (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (noAuthResult, DateTime.UtcNow.AddSeconds(30)));
                }

                Console.WriteLine($"[CUSTOM-JWT] ==================== AUTHENTICATION NO RESULT ====================");
                return noAuthResult;
            }

            var authHeaderValueFromHeader = authHeader.ToString();
            if (!authHeaderValueFromHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[CUSTOM-JWT] Authorization header is not Bearer token");
                var noBearerResult = AuthenticateResult.NoResult();
                Context.Items[AuthResultKey] = noBearerResult;

                // Cache the no-bearer result briefly (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (noBearerResult, DateTime.UtcNow.AddSeconds(30)));
                }

                return noBearerResult;
            }

            var token = authHeaderValueFromHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[CUSTOM-JWT] Bearer token is empty");
                var emptyTokenResult = AuthenticateResult.NoResult();
                Context.Items[AuthResultKey] = emptyTokenResult;

                // Cache the empty token result briefly (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (emptyTokenResult, DateTime.UtcNow.AddSeconds(30)));
                }

                return emptyTokenResult;
            }

            try
            {
                // Validate the JWT token manually using the same configuration as the original JWT setup
                var tokenHandler = new JwtSecurityTokenHandler();

                // Get JWT settings from configuration
                var jwtSettings = _configuration.GetSection("JWT");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey is not configured");
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "DecorStore",
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"] ?? "DecorStoreClients",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5")),
                    // Fix role claim mapping for authorization
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                Console.WriteLine($"[CUSTOM-JWT] Token validation succeeded for user: {principal.Identity?.Name}");
                Console.WriteLine($"[CUSTOM-JWT] Principal IsAuthenticated: {principal.Identity?.IsAuthenticated}");

                // CRITICAL: Ensure HttpContext.User is properly set
                Context.User = principal;

                Console.WriteLine($"[CUSTOM-JWT] HttpContext.User set to: {Context.User?.Identity?.Name}");
                Console.WriteLine($"[CUSTOM-JWT] HttpContext.User IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");

                // Log all claims for debugging
                var claims = principal.Claims.ToList();
                Console.WriteLine($"[CUSTOM-JWT] Total claims: {claims.Count}");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"[CUSTOM-JWT] Claim: {claim.Type} = {claim.Value}");
                }

                // Verify role claims specifically
                var roleClaims = principal.FindAll(ClaimTypes.Role).ToList();
                Console.WriteLine($"[CUSTOM-JWT] Role claims found: {roleClaims.Count}");
                foreach (var roleClaim in roleClaims)
                {
                    Console.WriteLine($"[CUSTOM-JWT] Role: {roleClaim.Value}");
                }

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                var result = AuthenticateResult.Success(ticket);

                // Cache the result for this request
                Context.Items[AuthResultKey] = result;

                // Cache globally for a short time (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (result, DateTime.UtcNow.AddMinutes(1)));
                }

                Console.WriteLine($"[CUSTOM-JWT] ==================== AUTHENTICATION SUCCESS ====================");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CUSTOM-JWT] Token validation failed: {ex.Message}");
                var failureResult = AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
                Context.Items[AuthResultKey] = failureResult;

                // Cache the failure result briefly (skip in test environment)
                if (!isTestEnvironment)
                {
                    _globalCache.TryAdd(cacheKey, (failureResult, DateTime.UtcNow.AddSeconds(30)));
                }

                return failureResult;
            }
        }



        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Console.WriteLine($"[CUSTOM-JWT] HandleChallengeAsync called for path: {Request.Path}");
            Console.WriteLine($"[CUSTOM-JWT] Current HttpContext.User: {Context.User?.Identity?.Name}");
            Console.WriteLine($"[CUSTOM-JWT] Current HttpContext.User IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");

            // Check if we have a cached successful authentication result
            if (Context.Items.TryGetValue(AuthResultKey, out var cachedResult) && cachedResult is AuthenticateResult cached && cached.Succeeded)
            {
                Console.WriteLine($"[CUSTOM-JWT] Found cached successful authentication, skipping challenge");
                return Task.CompletedTask;
            }

            return base.HandleChallengeAsync(properties);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Console.WriteLine($"[CUSTOM-JWT] HandleForbiddenAsync called for path: {Request.Path}");
            Console.WriteLine($"[CUSTOM-JWT] Current HttpContext.User: {Context.User?.Identity?.Name}");
            Console.WriteLine($"[CUSTOM-JWT] Current HttpContext.User IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");
            
            return base.HandleForbiddenAsync(properties);
        }
    }
}

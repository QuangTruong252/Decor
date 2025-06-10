using DecorStore.API.Common;
using DecorStore.API.Interfaces;
using DecorStore.API.Interfaces.Services;
using System.Security.Cryptography.X509Certificates;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Services
{
    /// <summary>
    /// Service for managing service-to-service authentication
    /// </summary>
    public interface IServiceAuthenticationService
    {
        Task<Result<ServiceAuthToken>> AuthenticateServiceAsync(string serviceName, X509Certificate2 certificate);
        Task<Result<bool>> ValidateServiceTokenAsync(string token, string? requiredService = null);
        Task<Result<ServiceIdentity>> GetServiceIdentityAsync(string token);
        Task<Result<bool>> ValidateMutualTlsAsync(X509Certificate2 clientCertificate, string serviceName);
        Task<Result<string>> GenerateServiceTokenAsync(string serviceName, TimeSpan? expiry = null);
        Task<Result> RevokeServiceTokenAsync(string token);
        Task<Result<List<ServiceCertificate>>> GetValidCertificatesAsync(string serviceName);
        Task<Result> RegisterServiceAsync(ServiceRegistrationRequest request);
        Task<Result> UpdateServiceAsync(string serviceName, ServiceUpdateRequest request);
        Task<Result> DeregisterServiceAsync(string serviceName);
        Task<Result<ServiceAuthAudit>> GetServiceAuthAuditAsync(string serviceName, DateTime? from = null, DateTime? to = null);
    }

    public class ServiceAuthenticationService : IServiceAuthenticationService
    {
        private readonly ILogger<ServiceAuthenticationService> _logger;
        private readonly ISecurityEventLogger _securityLogger;
        private readonly ServiceAuthSettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenService _tokenService;

        public ServiceAuthenticationService(
            ILogger<ServiceAuthenticationService> logger,
            ISecurityEventLogger securityLogger,
            IOptions<ServiceAuthSettings> settings,
            IUnitOfWork unitOfWork,
            IJwtTokenService tokenService)
        {
            _logger = logger;
            _securityLogger = securityLogger;
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<Result<ServiceAuthToken>> AuthenticateServiceAsync(string serviceName, X509Certificate2 certificate)
        {
            try
            {
                _logger.LogInformation("Authenticating service {ServiceName} with certificate {Thumbprint}", 
                    serviceName, certificate?.Thumbprint);

                // Validate service registration
                var serviceValidation = await ValidateServiceRegistrationAsync(serviceName);
                if (!serviceValidation.IsSuccess)
                {
                    await _securityLogger.LogSystemEventAsync("ServiceAuthFailed", 
                        $"Service authentication failed for {serviceName}: Service not registered", false);
                    return Result<ServiceAuthToken>.Failure("Service not registered");
                }

                // Validate certificate
                var certValidation = await ValidateServiceCertificateAsync(serviceName, certificate);
                if (!certValidation.IsSuccess)
                {
                    await _securityLogger.LogSystemEventAsync("ServiceAuthFailed", 
                        $"Service authentication failed for {serviceName}: Invalid certificate", false);
                    return Result<ServiceAuthToken>.Failure("Invalid certificate");
                }

                // Generate service token
                var tokenResult = await GenerateServiceTokenAsync(serviceName, TimeSpan.FromHours(_settings.DefaultTokenExpiryHours));
                if (!tokenResult.IsSuccess)
                {
                    return Result<ServiceAuthToken>.Failure("Failed to generate service token");
                }

                var authToken = new ServiceAuthToken
                {
                    Token = tokenResult.Data,
                    ServiceName = serviceName,
                    ExpiresAt = DateTime.UtcNow.AddHours(_settings.DefaultTokenExpiryHours),
                    IssuedAt = DateTime.UtcNow,
                    CertificateThumbprint = certificate?.Thumbprint,
                    Scopes = await GetServiceScopesAsync(serviceName)
                };

                // Log successful authentication
                await _securityLogger.LogSystemEventAsync("ServiceAuthSuccess", 
                    $"Service {serviceName} authenticated successfully", true, 
                    new { ServiceName = serviceName, CertificateThumbprint = certificate?.Thumbprint }.ToString());

                return Result<ServiceAuthToken>.Success(authToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating service {ServiceName}", serviceName);
                await _securityLogger.LogSystemEventAsync("ServiceAuthError", 
                    $"Service authentication error for {serviceName}: {ex.Message}", false);
                return Result<ServiceAuthToken>.Failure("Service authentication failed");
            }
        }

        public async Task<Result<bool>> ValidateServiceTokenAsync(string token, string? requiredService = null)
        {
            try
            {
                // Validate JWT token structure and signature
                var tokenValidation = await _tokenService.ValidateTokenAsync(token);
                if (!tokenValidation.IsSuccess)
                {
                    return Result<bool>.Success(false);
                }

                var claimsPrincipal = tokenValidation.Data;
                var serviceName = claimsPrincipal.FindFirst("service_name")?.Value;

                if (string.IsNullOrEmpty(serviceName))
                {
                    return Result<bool>.Success(false);
                }

                // Check if specific service is required
                if (!string.IsNullOrEmpty(requiredService) && !string.Equals(serviceName, requiredService, StringComparison.OrdinalIgnoreCase))
                {
                    await _securityLogger.LogAuthorizationFailureAsync(null, "service_auth", "validate_token", 
                        "system", $"Required service {requiredService}, but token is for {serviceName}");
                    return Result<bool>.Success(false);
                }

                // Validate service is still active
                var serviceValidation = await ValidateServiceRegistrationAsync(serviceName);
                if (!serviceValidation.IsSuccess)
                {
                    return Result<bool>.Success(false);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating service token");
                return Result<bool>.Failure("Failed to validate service token");
            }
        }

        public async Task<Result<ServiceIdentity>> GetServiceIdentityAsync(string token)
        {
            try
            {
                var tokenValidation = await _tokenService.ValidateTokenAsync(token);
                if (!tokenValidation.IsSuccess)
                {
                    return Result<ServiceIdentity>.Failure("Invalid token");
                }

                var claimsPrincipal = tokenValidation.Data;
                var serviceName = claimsPrincipal.FindFirst("service_name")?.Value;
                var scopes = claimsPrincipal.FindFirst("scopes")?.Value?.Split(',') ?? Array.Empty<string>();
                var issueDate = claimsPrincipal.FindFirst("iat")?.Value;

                if (string.IsNullOrEmpty(serviceName))
                {
                    return Result<ServiceIdentity>.Failure("Invalid service token");
                }

                var identity = new ServiceIdentity
                {
                    ServiceName = serviceName,
                    Scopes = scopes.ToList(),
                    IssuedAt = DateTime.UnixEpoch.AddSeconds(long.Parse(issueDate ?? "0")),
                    IsAuthenticated = true
                };

                return Result<ServiceIdentity>.Success(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service identity from token");
                return Result<ServiceIdentity>.Failure("Failed to get service identity");
            }
        }

        public async Task<Result<bool>> ValidateMutualTlsAsync(X509Certificate2 clientCertificate, string serviceName)
        {
            try
            {
                if (clientCertificate == null)
                {
                    return Result<bool>.Success(false);
                }

                // Validate certificate is not expired
                if (DateTime.Now < clientCertificate.NotBefore || DateTime.Now > clientCertificate.NotAfter)
                {
                    await _securityLogger.LogSecurityViolationAsync("ExpiredCertificate", null, "system", 
                        $"Expired certificate used for service {serviceName}", 8.0m);
                    return Result<bool>.Success(false);
                }

                // Validate certificate chain
                var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;

                bool isChainValid = chain.Build(clientCertificate);
                if (!isChainValid)
                {
                    await _securityLogger.LogSecurityViolationAsync("InvalidCertificateChain", null, "system", 
                        $"Invalid certificate chain for service {serviceName}", 7.0m);
                    return Result<bool>.Success(false);
                }

                // Validate certificate is registered for this service
                var certValidation = await ValidateServiceCertificateAsync(serviceName, clientCertificate);
                return Result<bool>.Success(certValidation.IsSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating mutual TLS for service {ServiceName}", serviceName);
                await _securityLogger.LogSecurityViolationAsync("MutualTlsError", null, "system", 
                    $"mTLS validation error for service {serviceName}: {ex.Message}", 6.0m);
                return Result<bool>.Failure("Failed to validate mutual TLS");
            }
        }

        public async Task<Result<string>> GenerateServiceTokenAsync(string serviceName, TimeSpan? expiry = null)
        {
            try
            {
                var tokenExpiry = expiry ?? TimeSpan.FromHours(_settings.DefaultTokenExpiryHours);
                var scopes = await GetServiceScopesAsync(serviceName);

                var claims = new List<Claim>
                {
                    new("service_name", serviceName),
                    new("token_type", "service"),
                    new("scopes", string.Join(",", scopes)),
                    new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new("exp", DateTimeOffset.UtcNow.Add(tokenExpiry).ToUnixTimeSeconds().ToString())
                };                var claimsDict = claims.ToDictionary(c => c.Type, c => (object)c.Value);
                claimsDict.Add("exp", DateTimeOffset.UtcNow.Add(tokenExpiry).ToUnixTimeSeconds());
                  // For service tokens, we'll generate them manually or use a different approach
                // Since this is service authentication, we'll generate a basic JWT directly
                var tokenResult = GenerateServiceJwtToken(serviceName, scopes.ToArray(), tokenExpiry);
                if (string.IsNullOrEmpty(tokenResult))
                {
                    return Result<string>.Failure("Failed to generate service token");
                }                // Store token reference for revocation
                await StoreServiceTokenAsync(serviceName, tokenResult, DateTime.UtcNow.Add(tokenExpiry));

                return Result<string>.Success(tokenResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating service token for {ServiceName}", serviceName);
                return Result<string>.Failure("Failed to generate service token");
            }
        }

        public async Task<Result> RevokeServiceTokenAsync(string token)
        {
            try
            {
                // Add token to blacklist
                var revokeResult = await _tokenService.RevokeTokenAsync(token);
                if (!revokeResult.IsSuccess)
                {
                    return Result.Failure("Failed to revoke token");
                }

                // Remove from service token storage
                await RemoveServiceTokenAsync(token);

                await _securityLogger.LogSystemEventAsync("ServiceTokenRevoked", 
                    "Service token revoked", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking service token");
                return Result.Failure("Failed to revoke service token");
            }
        }

        public async Task<Result<List<ServiceCertificate>>> GetValidCertificatesAsync(string serviceName)
        {
            try
            {
                // This would typically query a database or certificate store
                // For now, return a placeholder implementation
                var certificates = new List<ServiceCertificate>();

                return Result<List<ServiceCertificate>>.Success(certificates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting valid certificates for service {ServiceName}", serviceName);
                return Result<List<ServiceCertificate>>.Failure("Failed to get valid certificates");
            }
        }

        public async Task<Result> RegisterServiceAsync(ServiceRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Registering service {ServiceName}", request.ServiceName);

                // Validate service doesn't already exist
                var existingService = await GetServiceRegistrationAsync(request.ServiceName);
                if (existingService != null)
                {
                    return Result.Failure("Service already registered");
                }

                // Create service registration
                var registration = new ServiceRegistration
                {
                    ServiceName = request.ServiceName,
                    Description = request.Description,
                    Scopes = request.Scopes,
                    CertificateThumbprints = request.CertificateThumbprints,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Store service registration (this would use a repository)
                await StoreServiceRegistrationAsync(registration);

                await _securityLogger.LogSystemEventAsync("ServiceRegistered", 
                    $"Service {request.ServiceName} registered successfully", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering service {ServiceName}", request.ServiceName);
                return Result.Failure("Failed to register service");
            }
        }

        public async Task<Result> UpdateServiceAsync(string serviceName, ServiceUpdateRequest request)
        {
            try
            {
                var existingService = await GetServiceRegistrationAsync(serviceName);
                if (existingService == null)
                {
                    return Result.Failure("Service not found");
                }

                // Update service registration
                existingService.Description = request.Description ?? existingService.Description;
                existingService.Scopes = request.Scopes ?? existingService.Scopes;
                existingService.CertificateThumbprints = request.CertificateThumbprints ?? existingService.CertificateThumbprints;
                existingService.IsActive = request.IsActive ?? existingService.IsActive;
                existingService.UpdatedAt = DateTime.UtcNow;

                await UpdateServiceRegistrationAsync(existingService);

                await _securityLogger.LogSystemEventAsync("ServiceUpdated", 
                    $"Service {serviceName} updated successfully", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceName}", serviceName);
                return Result.Failure("Failed to update service");
            }
        }

        public async Task<Result> DeregisterServiceAsync(string serviceName)
        {
            try
            {
                // Mark service as inactive
                var existingService = await GetServiceRegistrationAsync(serviceName);
                if (existingService == null)
                {
                    return Result.Failure("Service not found");
                }

                existingService.IsActive = false;
                existingService.UpdatedAt = DateTime.UtcNow;

                await UpdateServiceRegistrationAsync(existingService);

                // Revoke all active tokens for this service
                await RevokeAllServiceTokensAsync(serviceName);

                await _securityLogger.LogSystemEventAsync("ServiceDeregistered", 
                    $"Service {serviceName} deregistered successfully", true);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deregistering service {ServiceName}", serviceName);
                return Result.Failure("Failed to deregister service");
            }
        }

        public async Task<Result<ServiceAuthAudit>> GetServiceAuthAuditAsync(string serviceName, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                // This would query audit logs for the service
                var audit = new ServiceAuthAudit
                {
                    ServiceName = serviceName,
                    From = from ?? DateTime.UtcNow.AddDays(-30),
                    To = to ?? DateTime.UtcNow,
                    TotalAuthAttempts = 0,
                    SuccessfulAuths = 0,
                    FailedAuths = 0,
                    TokensIssued = 0,
                    TokensRevoked = 0,
                    Events = new List<ServiceAuthEvent>()
                };

                return Result<ServiceAuthAudit>.Success(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service auth audit for {ServiceName}", serviceName);
                return Result<ServiceAuthAudit>.Failure("Failed to get service auth audit");
            }
        }

        // Private helper methods
        private async Task<Result> ValidateServiceRegistrationAsync(string serviceName)
        {
            var registration = await GetServiceRegistrationAsync(serviceName);
            if (registration == null || !registration.IsActive)
            {
                return Result.Failure("Service not registered or inactive");
            }
            return Result.Success();
        }

        private async Task<Result> ValidateServiceCertificateAsync(string serviceName, X509Certificate2 certificate)
        {
            var registration = await GetServiceRegistrationAsync(serviceName);
            if (registration?.CertificateThumbprints?.Contains(certificate.Thumbprint) != true)
            {
                return Result.Failure("Certificate not registered for service");
            }
            return Result.Success();
        }

        private async Task<List<string>> GetServiceScopesAsync(string serviceName)
        {
            var registration = await GetServiceRegistrationAsync(serviceName);
            return registration?.Scopes ?? new List<string> { "basic" };
        }

        // These would typically interact with a database
        private async Task<ServiceRegistration?> GetServiceRegistrationAsync(string serviceName)
        {
            // Placeholder - would query database
            await Task.Delay(1);
            return null;
        }

        private async Task StoreServiceRegistrationAsync(ServiceRegistration registration)
        {
            // Placeholder - would store in database
            await Task.Delay(1);
        }

        private async Task UpdateServiceRegistrationAsync(ServiceRegistration registration)
        {
            // Placeholder - would update database
            await Task.Delay(1);
        }

        private async Task StoreServiceTokenAsync(string serviceName, string token, DateTime expiry)
        {
            // Placeholder - would store token reference
            await Task.Delay(1);
        }

        private async Task RemoveServiceTokenAsync(string token)
        {
            // Placeholder - would remove token reference
            await Task.Delay(1);
        }        private async Task RevokeAllServiceTokensAsync(string serviceName)
        {
            // Placeholder - would revoke all tokens for service
            await Task.Delay(1);
        }

        private string GenerateServiceJwtToken(string serviceName, string[] scopes, TimeSpan expiry)
        {
            try
            {
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var key = System.Text.Encoding.ASCII.GetBytes(_settings.SharedSecret ?? "default-key-please-configure");

                var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[]
                    {
                        new System.Security.Claims.Claim("service_name", serviceName),
                        new System.Security.Claims.Claim("token_type", "service"),
                        new System.Security.Claims.Claim("scopes", string.Join(",", scopes)),
                        new System.Security.Claims.Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    }),
                    Expires = DateTime.UtcNow.Add(expiry),
                    SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                        new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), 
                        Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    // Supporting classes and enums
    public class ServiceAuthSettings
    {
        public int DefaultTokenExpiryHours { get; set; } = 24;
        public bool RequireMutualTls { get; set; } = true;
        public bool ValidateCertificateChain { get; set; } = true;
        public List<string> TrustedCertificateAuthorities { get; set; } = new();
        public int MaxTokensPerService { get; set; } = 10;
        public string SharedSecret { get; set; } = "default-key-please-configure-in-production";
    }

    public class ServiceAuthToken
    {
        public string Token { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? CertificateThumbprint { get; set; }
        public List<string> Scopes { get; set; } = new();
    }

    public class ServiceIdentity
    {
        public string ServiceName { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public DateTime IssuedAt { get; set; }
        public bool IsAuthenticated { get; set; }
    }

    public class ServiceCertificate
    {
        public string Thumbprint { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServiceRegistration
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public List<string> CertificateThumbprints { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ServiceRegistrationRequest
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public List<string> CertificateThumbprints { get; set; } = new();
    }

    public class ServiceUpdateRequest
    {
        public string? Description { get; set; }
        public List<string>? Scopes { get; set; }
        public List<string>? CertificateThumbprints { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ServiceAuthAudit
    {
        public string ServiceName { get; set; } = string.Empty;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalAuthAttempts { get; set; }
        public int SuccessfulAuths { get; set; }
        public int FailedAuths { get; set; }
        public int TokensIssued { get; set; }
        public int TokensRevoked { get; set; }
        public List<ServiceAuthEvent> Events { get; set; } = new();
    }

    public class ServiceAuthEvent
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}

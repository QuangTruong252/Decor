using Microsoft.AspNetCore.Authorization;
using DecorStore.API.Models.Authorization;
using DecorStore.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DecorStore.API.Services.Authorization
{
    /// <summary>
    /// Authorization handler for resource ownership validation
    /// </summary>
    public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ResourceOwnerAuthorizationHandler> _logger;

        public ResourceOwnerAuthorizationHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ResourceOwnerAuthorizationHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnerRequirement requirement)
        {
            try
            {
                var user = context.User;
                if (!user.Identity?.IsAuthenticated == true)
                {
                    context.Fail();
                    return;
                }

                // Admin users can access all resources
                if (user.IsInRole("Admin"))
                {
                    context.Succeed(requirement);
                    return;
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    context.Fail();
                    return;
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    context.Fail();
                    return;
                }

                // Get resource ID from route values
                var resourceIdValue = httpContext.Request.RouteValues[requirement.ResourceIdParameter]?.ToString();
                if (!int.TryParse(resourceIdValue, out var resourceId))
                {
                    context.Fail();
                    return;
                }

                // Check ownership based on resource type
                var isOwner = requirement.ResourceType.ToLower() switch
                {
                    "customer" => await IsCustomerOwnerAsync(userId, resourceId),
                    "order" => await IsOrderOwnerAsync(userId, resourceId),
                    "cart" => await IsCartOwnerAsync(userId, resourceId),
                    "review" => await IsReviewOwnerAsync(userId, resourceId),
                    _ => false
                };

                if (isOwner)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResourceOwnerAuthorizationHandler");
                context.Fail();
            }
        }

        private async Task<bool> IsCustomerOwnerAsync(int userId, int customerId)
        {
            // Customer model doesn't have UserId, so we need to check if the user has access
            // For now, we'll allow access if customer exists (you may need to implement proper logic)
            var customer = await _unitOfWork.Context.Set<Models.Customer>()
                .FirstOrDefaultAsync(c => c.Id == customerId);
            return customer != null; // TODO: Implement proper customer-user relationship
        }

        private async Task<bool> IsOrderOwnerAsync(int userId, int orderId)
        {
            var order = await _unitOfWork.Context.Set<Models.Order>()
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            return order?.Customer != null; // TODO: Implement proper order-user relationship
        }

        private async Task<bool> IsCartOwnerAsync(int userId, int cartId)
        {
            var cart = await _unitOfWork.Context.Set<Models.Cart>()
                .FirstOrDefaultAsync(c => c.Id == cartId);
            return cart?.UserId == userId;
        }

        private async Task<bool> IsReviewOwnerAsync(int userId, int reviewId)
        {
            var review = await _unitOfWork.Context.Set<Models.Review>()
                .FirstOrDefaultAsync(r => r.Id == reviewId);
            return review?.UserId == userId;
        }
    }

    /// <summary>
    /// Authorization handler for minimum age validation
    /// </summary>
    public class MinimumAgeAuthorizationHandler : AuthorizationHandler<MinimumAgeRequirement>
    {
        private readonly ILogger<MinimumAgeAuthorizationHandler> _logger;

        public MinimumAgeAuthorizationHandler(ILogger<MinimumAgeAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MinimumAgeRequirement requirement)
        {
            try
            {
                var dateOfBirthClaim = context.User.FindFirst("date_of_birth")?.Value;
                if (string.IsNullOrEmpty(dateOfBirthClaim))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                if (DateTime.TryParse(dateOfBirthClaim, out var dateOfBirth))
                {
                    var age = DateTime.Today.Year - dateOfBirth.Year;
                    if (dateOfBirth > DateTime.Today.AddYears(-age))
                        age--;

                    if (age >= requirement.MinimumAge)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MinimumAgeAuthorizationHandler");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authorization handler for business hours validation
    /// </summary>
    public class BusinessHoursAuthorizationHandler : AuthorizationHandler<BusinessHoursRequirement>
    {
        private readonly ILogger<BusinessHoursAuthorizationHandler> _logger;

        public BusinessHoursAuthorizationHandler(ILogger<BusinessHoursAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BusinessHoursRequirement requirement)
        {
            try
            {
                var now = DateTime.Now;
                var currentTime = now.TimeOfDay;
                var currentDay = now.DayOfWeek;

                // Check if current day is allowed
                if (!requirement.AllowedDays.Contains(currentDay))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Check if current time is within business hours
                if (currentTime >= requirement.StartTime && currentTime <= requirement.EndTime)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BusinessHoursAuthorizationHandler");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authorization handler for geolocation validation
    /// </summary>
    public class GeolocationAuthorizationHandler : AuthorizationHandler<GeolocationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GeolocationAuthorizationHandler> _logger;

        public GeolocationAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<GeolocationAuthorizationHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            GeolocationRequirement requirement)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Get country from headers (would typically come from a geolocation service)
                var country = httpContext.Request.Headers["X-Country-Code"].ToString();
                if (string.IsNullOrEmpty(country))
                {
                    // Default to allow if no country information
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // Check blocked countries first
                if (requirement.BlockedCountries.Any() && requirement.BlockedCountries.Contains(country, StringComparer.OrdinalIgnoreCase))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Check allowed countries
                if (requirement.AllowedCountries.Any())
                {
                    if (requirement.AllowedCountries.Contains(country, StringComparer.OrdinalIgnoreCase))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                else
                {
                    // If no allowed countries specified, allow all except blocked
                    context.Succeed(requirement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GeolocationAuthorizationHandler");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authorization handler for two-factor authentication validation
    /// </summary>
    public class TwoFactorAuthorizationHandler : AuthorizationHandler<TwoFactorRequirement>
    {
        private readonly ILogger<TwoFactorAuthorizationHandler> _logger;

        public TwoFactorAuthorizationHandler(ILogger<TwoFactorAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TwoFactorRequirement requirement)
        {
            try
            {
                var user = context.User;

                // Check if user has 2FA enabled claim
                var twoFactorClaim = user.FindFirst("two_factor_enabled")?.Value;
                if (string.IsNullOrEmpty(twoFactorClaim) || !bool.TryParse(twoFactorClaim, out var twoFactorEnabled) || !twoFactorEnabled)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Check recent authentication if required
                if (requirement.RequireRecentAuthentication)
                {
                    var authTimeClaim = user.FindFirst("auth_time")?.Value;
                    if (string.IsNullOrEmpty(authTimeClaim) || !long.TryParse(authTimeClaim, out var authTime))
                    {
                        context.Fail();
                        return Task.CompletedTask;
                    }

                    var authDateTime = DateTimeOffset.FromUnixTimeSeconds(authTime).DateTime;
                    if (DateTime.UtcNow - authDateTime > requirement.MaxAuthenticationAge)
                    {
                        context.Fail();
                        return Task.CompletedTask;
                    }
                }

                context.Succeed(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TwoFactorAuthorizationHandler");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Authorization handler for account status validation
    /// </summary>
    public class AccountStatusAuthorizationHandler : AuthorizationHandler<AccountStatusRequirement>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountStatusAuthorizationHandler> _logger;

        public AccountStatusAuthorizationHandler(
            IUnitOfWork unitOfWork,
            ILogger<AccountStatusAuthorizationHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AccountStatusRequirement requirement)
        {
            try
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    context.Fail();
                    return;
                }

                var user = await _unitOfWork.Context.Set<Models.User>()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    context.Fail();
                    return;
                }

                var userStatus = user.Status ?? "Active";

                // Check forbidden statuses
                if (requirement.ForbiddenStatuses.Contains(userStatus, StringComparer.OrdinalIgnoreCase))
                {
                    context.Fail();
                    return;
                }

                // Check required statuses
                if (requirement.RequiredStatuses.Contains(userStatus, StringComparer.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AccountStatusAuthorizationHandler");
                context.Fail();
            }
        }
    }

    /// <summary>
    /// Authorization handler for API key validation
    /// </summary>
    public class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiKeyAuthorizationHandler> _logger;

        public ApiKeyAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiKeyAuthorizationHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApiKeyRequirement requirement)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // Get API key from header
                var apiKey = httpContext.Request.Headers["X-API-Key"].ToString();
                if (string.IsNullOrEmpty(apiKey) && requirement.RequireValidKey)
                {
                    context.Fail();
                    return Task.CompletedTask;
                }

                // TODO: Implement actual API key validation logic
                // For now, just check if API key is present
                if (!string.IsNullOrEmpty(apiKey))
                {
                    // Get scopes from claims (would be set during API key validation)
                    var scopesClaim = context.User.FindFirst("api_scopes")?.Value;
                    var userScopes = scopesClaim?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

                    // Check if user has required scopes
                    if (requirement.RequiredScopes.Any())
                    {
                        var hasRequiredScopes = requirement.RequiredScopes.All(scope => 
                            userScopes.Contains(scope, StringComparer.OrdinalIgnoreCase));

                        if (hasRequiredScopes)
                        {
                            context.Succeed(requirement);
                        }
                        else
                        {
                            context.Fail();
                        }
                    }
                    else
                    {
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    context.Succeed(requirement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApiKeyAuthorizationHandler");
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

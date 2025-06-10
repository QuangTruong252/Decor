using System.Text;
using DecorStore.API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DecorStore.API.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {            // Configure and validate JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("JWT"));
            services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();
            
            // Enable options validation on startup
            services.AddOptions<JwtSettings>()
                .Bind(configuration.GetSection("JWT"))
                .ValidateOnStart();

            var jwtSettings = configuration.GetSection("JWT").Get<JwtSettings>() 
                ?? throw new InvalidOperationException("JWT settings are not configured");

            if (string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT:SecretKey is not configured");
            }

            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                options.SaveToken = jwtSettings.SaveToken;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes),
                    ValidateLifetime = jwtSettings.ValidateLifetime
                };

                // Add event handlers for debugging in development
                if (jwtSettings.EnableDebugEvents)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully");
                            var claims = context.Principal?.Claims.Select(c => new { c.Type, c.Value });
                            if (claims != null)
                            {
                                foreach (var claim in claims)
                                {
                                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                                }
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                }
            });

            // Add authorization policies and handlers
            services.AddAuthorizationPolicies();
            services.AddAuthorizationHandlers();

            return services;
        }

        public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });

                // Add more restrictive policy for production
                options.AddPolicy("Production", builder =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                        ?? new[] { "https://localhost", "https://your-production-domain.com" };
                    
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }
    }

    /// <summary>
    /// Validator for JwtSettings configuration
    /// </summary>
    public class JwtSettingsValidator : IValidateOptions<JwtSettings>
    {
        public ValidateOptionsResult Validate(string? name, JwtSettings options)
        {
            var failures = new List<string>();

            if (string.IsNullOrEmpty(options.SecretKey))
            {
                failures.Add("JWT SecretKey cannot be null or empty");
            }
            else if (options.SecretKey.Length < 32)
            {
                failures.Add("JWT SecretKey must be at least 32 characters long");
            }

            if (string.IsNullOrEmpty(options.Issuer))
            {
                failures.Add("JWT Issuer cannot be null or empty");
            }

            if (string.IsNullOrEmpty(options.Audience))
            {
                failures.Add("JWT Audience cannot be null or empty");
            }

            if (options.AccessTokenExpirationMinutes <= 0)
            {
                failures.Add("JWT AccessTokenExpirationMinutes must be greater than 0");
            }

            if (options.RefreshTokenExpirationDays <= 0)
            {
                failures.Add("JWT RefreshTokenExpirationDays must be greater than 0");
            }

            return failures.Count > 0 
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}

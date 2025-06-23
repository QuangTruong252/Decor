using Microsoft.OpenApi.Models;

namespace DecorStore.API.Extensions.Infrastructure
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "DecorStore API", 
                    Version = "v1",
                    Description = "A comprehensive API for managing a decoration store",
                    Contact = new OpenApiContact
                    {
                        Name = "DecorStore Team",
                        Email = "support@decorstore.com"
                    }
                });

                // Configure Swagger to use JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Include XML comments if available
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static WebApplication UseSwaggerMiddleware(this WebApplication app)
        {
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DecorStore API v1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                    c.EnableFilter();
                    c.ShowExtensions();
                    c.EnableValidator();
                });
            }

            return app;
        }
    }
}

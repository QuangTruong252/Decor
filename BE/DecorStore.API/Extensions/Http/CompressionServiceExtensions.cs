using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace DecorStore.API.Extensions.Http
{
    /// <summary>
    /// Extension methods for configuring response compression services
    /// </summary>
    public static class CompressionServiceExtensions
    {
        /// <summary>
        /// Adds and configures response compression services
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection for chaining</returns>
        public static IServiceCollection AddResponseCompressionServices(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                // Enable compression for all responses
                options.EnableForHttps = true;
                
                // Add providers in order of preference
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                
                // Configure MIME types that should be compressed
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/json",
                    "application/json; charset=utf-8",
                    "application/xml",
                    "application/xml; charset=utf-8",
                    "text/json",
                    "text/xml",
                    "application/javascript",
                    "text/css",
                    "text/html",
                    "text/plain",
                    "image/svg+xml"
                });
            });

            // Configure Brotli compression
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            // Configure Gzip compression
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            return services;
        }
    }
}

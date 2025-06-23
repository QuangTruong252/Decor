using DecorStore.API.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace DecorStore.API.Extensions.Http
{
    public static class FileStorageMiddlewareExtensions
    {
        public static WebApplication UseFileStorageMiddleware(this WebApplication app)
        {
            var fileStorageSettings = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;
            
            // Setup upload directories
            var uploadPath = Path.Combine(app.Environment.ContentRootPath, fileStorageSettings.UploadPath);
            var thumbnailPath = Path.Combine(uploadPath, fileStorageSettings.ThumbnailPath);
            
            EnsureDirectoryExists(uploadPath);
            EnsureDirectoryExists(thumbnailPath);

            // Configure static files for uploads
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadPath),
                RequestPath = fileStorageSettings.BaseUrl,
                OnPrepareResponse = context =>
                {
                    // Add cache headers for images
                    var headers = context.Context.Response.Headers;
                    headers.CacheControl = "public,max-age=31536000"; // 1 year
                    headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");
                }
            });

            // Configure static files for thumbnails
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(thumbnailPath),
                RequestPath = fileStorageSettings.ThumbnailUrl,
                OnPrepareResponse = context =>
                {
                    // Add cache headers for thumbnails
                    var headers = context.Context.Response.Headers;
                    headers.CacheControl = "public,max-age=31536000"; // 1 year
                    headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");
                }
            });

            return app;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"Created directory: {path}");
            }
        }
    }
}

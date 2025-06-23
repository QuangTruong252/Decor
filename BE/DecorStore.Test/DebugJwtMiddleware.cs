using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DecorStore.Test
{
    public class DebugJwtMiddleware
    {
        private readonly RequestDelegate _next;

        public DebugJwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Debug: Check if this is an auth request
            if (context.Request.Path.StartsWithSegments("/api/Auth/user"))
            {
                Console.WriteLine($"[DEBUG] Request to {context.Request.Path}");
                Console.WriteLine($"[DEBUG] Authorization Header: {context.Request.Headers.Authorization}");
                Console.WriteLine($"[DEBUG] User Identity: {context.User?.Identity?.Name ?? "null"}");
                Console.WriteLine($"[DEBUG] User IsAuthenticated: {context.User?.Identity?.IsAuthenticated ?? false}");
            }

            await _next(context);

            // Debug: Check response
            if (context.Request.Path.StartsWithSegments("/api/Auth/user"))
            {
                Console.WriteLine($"[DEBUG] Response Status: {context.Response.StatusCode}");
                Console.WriteLine($"[DEBUG] User Identity After: {context.User?.Identity?.Name ?? "null"}");
                Console.WriteLine($"[DEBUG] User IsAuthenticated After: {context.User?.Identity?.IsAuthenticated ?? false}");
            }
        }
    }
}

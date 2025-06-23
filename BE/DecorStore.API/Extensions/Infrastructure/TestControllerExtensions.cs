using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace DecorStore.API.Extensions.Infrastructure
{
    public static class TestControllerExtensions
    {
        public static IMvcBuilder AddTestControllers(this IMvcBuilder builder, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Only add test controllers in Development environment
                builder.AddApplicationPart(typeof(DecorStore.API.Controllers.Test.ConfigurationTestController).Assembly);
            }
            return builder;
        }
    }
}

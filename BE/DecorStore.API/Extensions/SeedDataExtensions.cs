using DecorStore.API.Data;
using Microsoft.EntityFrameworkCore;

namespace DecorStore.API.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedFurnitureDataAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    
                    // Ensure database is created and migrations are applied
                    await context.Database.MigrateAsync();
                    
                    // Seed furniture data
                    var seeder = new FurnitureDbSeeder(context);
                    await seeder.SeedFurnitureData();
                    
                    Console.WriteLine("Furniture data seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database with furniture data.");
                    Console.WriteLine($"Error seeding furniture data: {ex.Message}");
                }
            }
        }
    }
}

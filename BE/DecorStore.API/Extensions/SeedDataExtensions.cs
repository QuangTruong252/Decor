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

                    // Check if database exists and can be connected to
                    Console.WriteLine("Checking database connection...");
                    bool canConnect = false;
                    try
                    {
                        canConnect = await context.Database.CanConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error connecting to database: {ex.Message}");
                        return; // Exit if we can't connect
                    }

                    if (!canConnect)
                    {
                        Console.WriteLine("Cannot connect to database. Skipping seeding.");
                        return;
                    }

                    // Apply migrations instead of recreating the database
                    Console.WriteLine("Applying migrations...");
                    try
                    {
                        await context.Database.MigrateAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error applying migrations: {ex.Message}");
                        return; // Exit if migrations fail
                    }

                    // Seed furniture data
                    var seeder = new FurnitureDbSeeder(context);
                    await seeder.SeedFurnitureData();

                    // Update image paths
                    var imagePathUpdater = new ImagePathUpdater(context);
                    await imagePathUpdater.UpdateImagePaths();

                    Console.WriteLine("Furniture data seeding and image path updates completed successfully.");
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

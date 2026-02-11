using GirafAPI.Data;
using GirafAPI.Entities.Organizations;
using GirafAPI.Entities.Pictograms;
using GirafAPI.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GirafAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<GirafUser>>();

            // Create a user
            var user = await userManager.FindByNameAsync("user");
            if (user == null)
            {
                user = new GirafUser { UserName = "user", 
                                       Email = "user@user.com", 
                                       FirstName = "User", 
                                       LastName = "Userson", 
                                       Organizations = new List<Organization>() };
                await userManager.CreateAsync(user, "Password123");
            }
        }

        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            await dbContext.Database.MigrateAsync();
        }
        
        public static async Task AddDefaultPictograms(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("SeedData");

            if (dbContext.Pictograms.Any()) return;
            var defaultPictograms = new List<Pictogram>();
            
            var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
                ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
                : env.WebRootPath;
            var defaultPictogramDirectory = Path.Combine(webRoot, "images", "pictograms", "default");

            if (!Directory.Exists(defaultPictogramDirectory))
            {
                logger.LogWarning("Default pictogram directory not found: {Directory}", defaultPictogramDirectory);
                return;
            }

            var defaultPictogramFiles = Directory.GetFiles(defaultPictogramDirectory);
            if (defaultPictogramFiles.Length == 0)
            {
                logger.LogWarning("No default pictogram files found in {Directory}", defaultPictogramDirectory);
                return;
            }
                
            foreach (var file in defaultPictogramFiles)
            {
                var fileName = Path.GetFileName(file);
                var pictogram = new Pictogram
                {
                    OrganizationId = null,
                    PictogramName = Path.GetFileNameWithoutExtension(fileName),
                    PictogramUrl = Path.Combine("images", "pictograms", "default", fileName)
                };
                defaultPictograms.Add(pictogram);
            }

            dbContext.Pictograms.AddRange(defaultPictograms);
            await dbContext.SaveChangesAsync();
        }
    }
}
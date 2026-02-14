using GirafAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GirafDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}

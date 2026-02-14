using GirafAPI.Entities.Activities;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Data
{
    public class GirafDbContext : DbContext
    {
        public GirafDbContext(DbContextOptions<GirafDbContext> options) : base(options)
        {
        }

        public DbSet<Activity> Activities { get; set; }
    }
}

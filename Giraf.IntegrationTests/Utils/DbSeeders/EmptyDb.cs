using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public override void SeedData(GirafDbContext dbContext)
    {
    }
}

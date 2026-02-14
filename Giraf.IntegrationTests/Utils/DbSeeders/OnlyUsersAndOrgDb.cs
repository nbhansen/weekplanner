using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

/// <summary>
/// Seeds nothing — equivalent to no activities in the DB.
/// Named for backward compatibility with existing tests.
/// </summary>
public class OnlyUsersAndOrgDb : DbSeeder
{
    public override void SeedData(GirafDbContext dbContext)
    {
    }
}

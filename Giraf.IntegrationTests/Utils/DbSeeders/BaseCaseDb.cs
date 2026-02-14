using GirafAPI.Data;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public class BaseCaseDb : DbSeeder
{
    public override void SeedData(GirafDbContext dbContext)
    {
        // Seed a citizen activity (citizenId=1, pictogramId=1)
        SeedCitizenActivity(dbContext, citizenId: 1, pictogramId: 1);
        // Seed a grade activity (gradeId=1, pictogramId=1)
        SeedGradeActivity(dbContext, gradeId: 1, pictogramId: 1);
    }
}

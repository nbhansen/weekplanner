using GirafAPI.Data;
using GirafAPI.Entities.Activities;

namespace Giraf.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public List<Activity> Activities { get; } = new();

    public abstract void SeedData(GirafDbContext dbContext);

    public void SeedCitizenActivity(GirafDbContext dbContext, int citizenId, int? pictogramId = null)
    {
        var activity = new Activity
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            CitizenId = citizenId,
            PictogramId = pictogramId
        };

        dbContext.Activities.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }

    public void SeedGradeActivity(GirafDbContext dbContext, int gradeId, int? pictogramId = null)
    {
        var activity = new Activity
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
            EndTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
            IsCompleted = false,
            GradeId = gradeId,
            PictogramId = pictogramId
        };

        dbContext.Activities.Add(activity);
        dbContext.SaveChanges();
        Activities.Add(activity);
    }
}

using System.Net;
using System.Net.Http.Json;
using Giraf.IntegrationTests.Utils;
using Giraf.IntegrationTests.Utils.DbSeeders;
using GirafAPI.Data;
using GirafAPI.Entities.Activities.DTOs;
using Microsoft.Extensions.DependencyInjection;

namespace Giraf.IntegrationTests.Endpoints
{
    [Collection("IntegrationTests")]
    public class ActivityEndpointTests
    {
        #region GET /weekplan/ - Get all activities

        [Fact]
        public async Task GetAllActivities_ReturnsListOfActivities_WhenActivitiesExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var response = await client.GetAsync("/weekplan");

            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetAllActivities_ReturnsEmptyList_WhenNoActivitiesExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var response = await client.GetAsync("/weekplan");

            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.Empty(activities);
        }

        #endregion

        #region GET /weekplan/{citizenId:int} - Get activities for a citizen on a date

        [Fact]
        public async Task GetActivitiesForCitizenOnDate_ReturnsActivities_WhenActivitiesExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/weekplan/1?date={date}");

            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetActivitiesForCitizenOnDate_ReturnsEmptyList_WhenCitizenDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/weekplan/999?date={date}");

            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.Empty(activities);
        }

        #endregion

        #region GET /weekplan/grade/{gradeId:int} - Get activities for a grade on a date

        [Fact]
        public async Task GetActivitiesForGradeOnDate_ReturnsActivities_WhenActivitiesExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/weekplan/grade/1?date={date}");

            response.EnsureSuccessStatusCode();
            var activities = await response.Content.ReadFromJsonAsync<List<ActivityDTO>>();
            Assert.NotNull(activities);
            Assert.NotEmpty(activities);
        }

        [Fact]
        public async Task GetActivitiesForGradeOnDate_ReturnsNotFound_WhenNoActivities()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var date = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            var response = await client.GetAsync($"/weekplan/grade/999?date={date}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region GET /weekplan/activity/{id:int} - Get activity by ID

        [Fact]
        public async Task GetActivityById_ReturnsActivity_WhenActivityExists()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            int activityId = seeder.Activities[0].Id;
            var response = await client.GetAsync($"/weekplan/activity/{activityId}");

            response.EnsureSuccessStatusCode();
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(activityId, activityDto.ActivityId);
        }

        [Fact]
        public async Task GetActivityById_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var response = await client.GetAsync("/weekplan/activity/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/to-citizen/{citizenId:int} - Create activity for citizen

        [Fact]
        public async Task CreateActivityForCitizen_ReturnsCreated_WhenCitizenExists()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: 1
            );

            var response = await client.PostAsJsonAsync("/weekplan/to-citizen/1", newActivityDto);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(1, activityDto.PictogramId);
        }

        [Fact]
        public async Task CreateActivityForCitizen_ReturnsNotFound_WhenCitizenDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: null
            );

            // StubCoreClient returns false for id >= 101
            var response = await client.PostAsJsonAsync("/weekplan/to-citizen/999", newActivityDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/to-grade/{gradeId:int} - Create activity for grade

        [Fact]
        public async Task CreateActivityForGrade_ReturnsCreated_WhenGradeExists()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: 1
            );

            var response = await client.PostAsJsonAsync("/weekplan/to-grade/1", newActivityDto);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
        }

        [Fact]
        public async Task CreateActivityForGrade_ReturnsNotFound_WhenGradeDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var newActivityDto = new CreateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                PictogramId: null
            );

            var response = await client.PostAsJsonAsync("/weekplan/to-grade/999", newActivityDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PUT /weekplan/activity/{id:int} - Update activity

        [Fact]
        public async Task UpdateActivity_ReturnsOk_WhenActivityExists()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            int activityId = seeder.Activities[0].Id;

            var updateActivityDto = new UpdateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                IsCompleted: true,
                PictogramId: 1
            );

            var response = await client.PutAsJsonAsync($"/weekplan/activity/{activityId}", updateActivityDto);

            response.EnsureSuccessStatusCode();

            using var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var updatedActivity = await dbContext.Activities.FindAsync(activityId);
            Assert.NotNull(updatedActivity);
            Assert.True(updatedActivity.IsCompleted);
        }

        [Fact]
        public async Task UpdateActivity_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var updateDto = new UpdateActivityDTO
            (
                Date: DateOnly.FromDateTime(DateTime.UtcNow).ToString(),
                StartTime: TimeOnly.FromDateTime(DateTime.UtcNow).ToString(),
                EndTime: TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1)).ToString(),
                IsCompleted: false,
                PictogramId: null
            );

            var response = await client.PutAsJsonAsync("/weekplan/activity/999", updateDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region DELETE /weekplan/activity/{id:int} - Delete activity

        [Fact]
        public async Task DeleteActivity_ReturnsNoContent_WhenActivityExists()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            int activityId = seeder.Activities[0].Id;
            var response = await client.DeleteAsync($"/weekplan/activity/{activityId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var deletedActivity = await dbContext.Activities.FindAsync(activityId);
            Assert.Null(deletedActivity);
        }

        [Fact]
        public async Task DeleteActivity_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            var response = await client.DeleteAsync("/weekplan/activity/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region PUT /weekplan/activity/{id:int}/iscomplete - Set activity completion status

        [Fact]
        public async Task SetActivityCompletionStatus_ReturnsOk_WhenActivityExists()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            int activityId = seeder.Activities[0].Id;

            var response = await client.PutAsync($"/weekplan/activity/{activityId}/iscomplete?IsComplete=true", null);

            response.EnsureSuccessStatusCode();

            using var verificationScope = factory.Services.CreateScope();
            var dbContext = verificationScope.ServiceProvider.GetRequiredService<GirafDbContext>();
            var updatedActivity = await dbContext.Activities.FindAsync(activityId);
            Assert.NotNull(updatedActivity);
            Assert.True(updatedActivity.IsCompleted);
        }

        [Fact]
        public async Task SetActivityCompletionStatus_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory();
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var response = await client.PutAsync("/weekplan/activity/999/iscomplete?IsComplete=true", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST /weekplan/activity/assign-pictogram/{activityId:int}/{pictogramId:int} - Assign pictogram

        [Fact]
        public async Task AssignPictogram_ReturnsOk_WhenActivityAndPictogramExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            int activityId = seeder.Activities[0].Id;

            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{activityId}/5", null);

            response.EnsureSuccessStatusCode();
            var activityDto = await response.Content.ReadFromJsonAsync<ActivityDTO>();
            Assert.NotNull(activityDto);
            Assert.Equal(5, activityDto.PictogramId);
        }

        [Fact]
        public async Task AssignPictogram_ReturnsNotFound_WhenActivityDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new EmptyDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "admin");

            var response = await client.PostAsync("/weekplan/activity/assign-pictogram/999/1", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AssignPictogram_ReturnsNotFound_WhenPictogramDoesNotExist()
        {
            var factory = new GirafWebApplicationFactory(stubCoreClient: true);
            var seeder = new BaseCaseDb();
            var scope = factory.Services.CreateScope();
            factory.SeedDb(scope, seeder);
            var client = factory.CreateClient();
            client.AttachClaimsToken(role: "member");

            int activityId = seeder.Activities[0].Id;

            // StubCoreClient returns false for id >= 101
            var response = await client.PostAsync($"/weekplan/activity/assign-pictogram/{activityId}/999", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion
    }
}

using GirafAPI.Clients;
using GirafAPI.Data;
using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Activities.DTOs;
using GirafAPI.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GirafAPI.Endpoints;

public static class ActivityEndpoints
{
    private static string? GetAccessToken(HttpContext context)
    {
        var auth = context.Request.Headers.Authorization.ToString();
        if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return auth["Bearer ".Length..].Trim();
        return null;
    }

    public static RouteGroupBuilder MapActivityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("weekplan");

        // GET all activities (mainly for debugging)
        group.MapGet("/", async (GirafDbContext dbContext) =>
            {
                try
                {
                    var activities = await dbContext.Activities
                        .Select(a => a.ToDTO())
                        .AsNoTracking()
                        .ToListAsync();

                    return Results.Ok(activities);
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetAllActivities")
            .WithDescription("Gets all activities.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);


        // GET activities for one day for a citizen
        group.MapGet("/{citizenId:int}", async (int citizenId, string date, GirafDbContext dbContext) =>
            {
                try
                {
                    var parsedDate = DateOnly.Parse(date);

                    var activities = await dbContext.Activities
                        .Where(a => a.CitizenId == citizenId && a.Date == parsedDate)
                        .Select(a => a.ToDTO())
                        .AsNoTracking()
                        .ToListAsync();

                    return Results.Ok(activities);
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while retrieving activities.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivitiesForCitizenOnDate")
            .WithDescription("Gets activities for a specific citizen on a given date.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        // GET activities for one day for a grade
        group.MapGet("/grade/{gradeId:int}", async (int gradeId, string date, GirafDbContext dbContext) =>
            {
                try
                {
                    var parsedDate = DateOnly.Parse(date);

                    var activities = await dbContext.Activities
                        .Where(a => a.GradeId == gradeId && a.Date == parsedDate)
                        .Select(a => a.ToDTO())
                        .AsNoTracking()
                        .ToListAsync();

                    return activities.Count == 0 ? Results.NotFound() : Results.Ok(activities);
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while retrieving activities.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivitiesForGradeOnDate")
            .WithDescription("Gets activities for a specific grade on a given date.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<List<ActivityDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        // GET single activity by ID
        group.MapGet("/activity/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    var activity = await dbContext.Activities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == id);

                    return activity is null ? Results.NotFound("Activity not found.") : Results.Ok(activity.ToDTO());
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while retrieving the activity.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("GetActivityById")
            .WithDescription("Gets a specific activity by ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces<ActivityDTO>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        // POST new activity for citizen
        group.MapPost("/to-citizen/{citizenId:int}",
                async (int citizenId, CreateActivityDTO newActivityDto, GirafDbContext dbContext,
                    ICoreClient coreClient, HttpContext httpContext) =>
                {
                    try
                    {
                        var token = GetAccessToken(httpContext);
                        if (token is null)
                            return Results.Unauthorized();

                        if (!await coreClient.ValidateCitizenAsync(citizenId, token))
                            return Results.NotFound("Citizen not found.");

                        if (newActivityDto.PictogramId is not null &&
                            !await coreClient.ValidatePictogramAsync(newActivityDto.PictogramId.Value, token))
                            return Results.NotFound("Pictogram not found.");

                        var activity = newActivityDto.ToEntity();
                        activity.CitizenId = citizenId;

                        dbContext.Activities.Add(activity);
                        await dbContext.SaveChangesAsync();
                        return Results.Created($"/activity/{activity.Id}", activity.ToDTO());
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while creating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CreateActivityForCitizen")
            .WithDescription("Creates a new activity for a citizen.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Accepts<CreateActivityDTO>("application/json")
            .Produces<ActivityDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        // POST new activity for grade
        group.MapPost("/to-grade/{gradeId:int}",
                async (int gradeId, CreateActivityDTO newActivityDto, GirafDbContext dbContext,
                    ICoreClient coreClient, HttpContext httpContext) =>
                {
                    try
                    {
                        var token = GetAccessToken(httpContext);
                        if (token is null)
                            return Results.Unauthorized();

                        if (!await coreClient.ValidateGradeAsync(gradeId, token))
                            return Results.NotFound("Grade not found.");

                        if (newActivityDto.PictogramId is not null &&
                            !await coreClient.ValidatePictogramAsync(newActivityDto.PictogramId.Value, token))
                            return Results.NotFound("Pictogram not found.");

                        var activity = newActivityDto.ToEntity();
                        activity.GradeId = gradeId;

                        dbContext.Activities.Add(activity);
                        await dbContext.SaveChangesAsync();
                        return Results.Created($"/activity/{activity.Id}", activity.ToDTO());
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while creating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CreateActivityForGrade")
            .WithDescription("Creates a new activity for a grade.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Accepts<CreateActivityDTO>("application/json")
            .Produces<ActivityDTO>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        group.MapPost("/activity/copy-citizen/{citizenId:int}",
                async (int citizenId, string dateStr, string newDateStr, List<int> toCopyIds,
                    GirafDbContext dbContext) =>
                {
                    try
                    {
                        var sourceDate = DateOnly.Parse(dateStr);
                        var targetDate = DateOnly.Parse(newDateStr);

                        var activities = await dbContext.Activities
                            .Where(a => a.CitizenId == citizenId &&
                                (a.Date == sourceDate || toCopyIds.Contains(a.Id)))
                            .AsNoTracking()
                            .ToListAsync();

                        if (activities.Count is 0)
                        {
                            return Results.NotFound("No activities found for the given date.");
                        }

                        foreach (var a in activities)
                        {
                            var newActivity = new Activity
                            {
                                Date = targetDate,
                                StartTime = a.StartTime,
                                EndTime = a.EndTime,
                                IsCompleted = false,
                                CitizenId = citizenId,
                                PictogramId = a.PictogramId
                            };

                            dbContext.Activities.Add(newActivity);
                        }

                        await dbContext.SaveChangesAsync();
                        return Results.Ok("Activities successfully copied.");
                    }
                    catch (FormatException)
                    {
                        return Results.BadRequest(
                            "Invalid date format. Please provide the date in 'YYYY-MM-DD' format.");
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while copying the activities.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CopyActivityForCitizen")
            .WithDescription("Copies activities between days for a citizen")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/activity/copy-grade/{gradeId:int}",
                async (int gradeId, string dateStr, string newDateStr, List<int> toCopyIds, GirafDbContext dbContext) =>
                {
                    try
                    {
                        var sourceDate = DateOnly.Parse(dateStr);
                        var targetDate = DateOnly.Parse(newDateStr);

                        var activities = await dbContext.Activities
                            .Where(a => a.GradeId == gradeId &&
                                (a.Date == sourceDate || toCopyIds.Contains(a.Id)))
                            .AsNoTracking()
                            .ToListAsync();

                        if (activities.Count is 0)
                        {
                            return Results.NotFound("No activities found for the given date.");
                        }

                        foreach (var a in activities)
                        {
                            var newActivity = new Activity
                            {
                                Date = targetDate,
                                StartTime = a.StartTime,
                                EndTime = a.EndTime,
                                IsCompleted = false,
                                GradeId = gradeId,
                                PictogramId = a.PictogramId
                            };

                            dbContext.Activities.Add(newActivity);
                        }

                        await dbContext.SaveChangesAsync();
                        return Results.Ok("Activities successfully copied.");
                    }
                    catch (FormatException)
                    {
                        return Results.BadRequest(
                            "Invalid date format. Please provide the date in 'YYYY-MM-DD' format.");
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while copying the activities.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("CopyActivityForGrade")
            .WithDescription("Copies activities between days for a grade")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status200OK);

        // PUT updated activity
        group.MapPut("/activity/{id:int}",
                async (int id, UpdateActivityDTO updatedActivity, GirafDbContext dbContext,
                    ICoreClient coreClient, HttpContext httpContext) =>
                {
                    try
                    {
                        var activity = await dbContext.Activities.FindAsync(id);

                        if (activity is null)
                        {
                            return Results.NotFound("Activity not found.");
                        }

                        if (updatedActivity.PictogramId is not null)
                        {
                            var token = GetAccessToken(httpContext);
                            if (token is null)
                                return Results.Unauthorized();

                            if (!await coreClient.ValidatePictogramAsync(updatedActivity.PictogramId.Value, token))
                                return Results.BadRequest($"Pictogram with ID {updatedActivity.PictogramId} not found.");
                        }

                        dbContext.Entry(activity).CurrentValues.SetValues(updatedActivity.ToEntity(id));
                        await dbContext.SaveChangesAsync();

                        return Results.Ok();
                    }
                    catch (DbUpdateException)
                    {
                        return Results.BadRequest("Failed to update activity. Ensure the provided data is correct.");
                    }
                    catch (Exception)
                    {
                        return Results.Problem("An error occurred while updating the activity.",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("UpdateActivity")
            .WithDescription("Updates an existing activity using ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Accepts<UpdateActivityDTO>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


        // PUT IsComplete activity
        group.MapPut("/activity/{id:int}/iscomplete", async (int id, bool IsComplete, GirafDbContext dbContext) =>
            {
                try
                {
                    var activity = await dbContext.Activities.FindAsync(id);

                    if (activity is null)
                    {
                        return Results.NotFound("Activity not found.");
                    }

                    activity.IsCompleted = IsComplete;
                    await dbContext.SaveChangesAsync();

                    return Results.Ok();
                }
                catch (DbUpdateException)
                {
                    return Results.BadRequest("Failed to update activity status.");
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while updating the activity status.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("CompleteActivity")
            .WithDescription("Completes an existing activity using ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);


        // DELETE activity
        group.MapDelete("/activity/{id:int}", async (int id, GirafDbContext dbContext) =>
            {
                try
                {
                    var rowsAffected = await dbContext.Activities.Where(a => a.Id == id).ExecuteDeleteAsync();

                    if (rowsAffected == 0)
                    {
                        return Results.NotFound("Activity not found.");
                    }

                    return Results.NoContent();
                }
                catch (DbUpdateException)
                {
                    return Results.BadRequest("Failed to delete activity. Ensure the ID is correct.");
                }
                catch (Exception)
                {
                    return Results.Problem("An error occurred while deleting the activity.",
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("DeleteActivity")
            .WithDescription("Deletes an activity by ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/activity/assign-pictogram/{activityId:int}/{pictogramId:int}",
                async (int activityId, int pictogramId, GirafDbContext dbContext,
                    ICoreClient coreClient, HttpContext httpContext) =>
                {
                    try
                    {
                        var activity = await dbContext.Activities.FindAsync(activityId);

                        if (activity is null)
                        {
                            return Results.NotFound("Activity not found.");
                        }

                        var token = GetAccessToken(httpContext);
                        if (token is null)
                            return Results.Unauthorized();

                        if (!await coreClient.ValidatePictogramAsync(pictogramId, token))
                        {
                            return Results.NotFound("Pictogram not found.");
                        }

                        activity.PictogramId = pictogramId;
                        await dbContext.SaveChangesAsync();

                        return Results.Ok(activity.ToDTO());
                    }
                    catch (Exception ex)
                    {
                        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
                    }
                })
            .WithName("AssignPictogram")
            .WithDescription("Assigns a pictogram by ID.")
            .WithTags("Activities")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);


        return group;
    }
}

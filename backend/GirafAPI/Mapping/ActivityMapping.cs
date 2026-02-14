using GirafAPI.Entities.Activities;
using GirafAPI.Entities.Activities.DTOs;

namespace GirafAPI.Mapping;

public static class ActivityMapping
{
    public static Activity ToEntity(this CreateActivityDTO activityDto)
    {
        return new Activity
        {
            Date = DateOnly.Parse(activityDto.Date),
            StartTime = TimeOnly.Parse(activityDto.StartTime),
            EndTime = TimeOnly.Parse(activityDto.EndTime),
            IsCompleted = false,
            PictogramId = activityDto.PictogramId
        };
    }

    public static Activity ToEntity(this UpdateActivityDTO activityDto, int id)
    {
        return new Activity
        {
            Id = id,
            Date = DateOnly.Parse(activityDto.Date),
            StartTime = TimeOnly.Parse(activityDto.StartTime),
            EndTime = TimeOnly.Parse(activityDto.EndTime),
            IsCompleted = activityDto.IsCompleted,
            PictogramId = activityDto.PictogramId
        };
    }

    public static ActivityDTO ToDTO(this Activity activity)
    {
        return new ActivityDTO(
            activity.Id,
            activity.Date.ToString("yyyy-MM-dd"),
            activity.StartTime.ToString("HH:mm"),
            activity.EndTime.ToString("HH:mm"),
            activity.IsCompleted,
            activity.PictogramId
        );
    }
}

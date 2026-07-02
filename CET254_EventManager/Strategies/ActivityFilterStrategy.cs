using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Strategies
{
    // Strategy pattern: filter by activity id, activity type, or both.
    public class ActivityFilterStrategy : IEventFilterStrategy
    {
        public IQueryable<Event> Apply(IQueryable<Event> events, EventFilterViewModel filter)
        {
            if (filter.ActivityId.HasValue)
            {
                var activityId = filter.ActivityId.Value;

                events = events.Where(eventItem => eventItem.EventActivities
                    .Any(eventActivity => eventActivity.ActivityId == activityId));
            }

            if (!string.IsNullOrWhiteSpace(filter.ActivityType))
            {
                if (!Enum.TryParse<ActivityType>(filter.ActivityType.Trim(), ignoreCase: true, out var activityType))
                {
                    return events.Where(_ => false);
                }

                events = events.Where(eventItem => eventItem.EventActivities
                    .Any(eventActivity => eventActivity.Activity.ActivityType == activityType));
            }

            return events;
        }
    }
}

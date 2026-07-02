using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Interfaces
{
    // Service contract for event queries and management.
    public interface IEventService
    {
        Task<List<Event>> GetUpcomingEventsAsync();

        Task<List<Event>> GetUpcomingEventsAsync(DateTime fromDate);

        Task<List<Event>> GetFilteredUpcomingEventsAsync(EventFilterViewModel filter);

        Task<Event> GetEventDetailsAsync(int eventId);

        Task<Event> CreateEventAsync(Event eventItem);

        Task<Event> CreateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds);

        Task UpdateEventAsync(Event eventItem);

        Task UpdateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds);

        Task DeleteEventAsync(int eventId);

        Task<int> GetTotalEventsCountAsync();
    }
}

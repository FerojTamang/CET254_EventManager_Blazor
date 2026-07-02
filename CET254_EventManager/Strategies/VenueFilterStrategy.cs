using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Strategies
{
    // Strategy pattern: filter events by selected venue.
    public class VenueFilterStrategy : IEventFilterStrategy
    {
        public IQueryable<Event> Apply(IQueryable<Event> events, EventFilterViewModel filter)
        {
            if (!filter.VenueId.HasValue)
            {
                return events;
            }

            var venueId = filter.VenueId.Value;

            return events.Where(eventItem => eventItem.EventVenues
                .Any(eventVenue => eventVenue.VenueId == venueId));
        }
    }
}

using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Strategies
{
    // Strategy pattern: apply the selected event date filter.
    public class DateFilterStrategy : IEventFilterStrategy
    {
        public IQueryable<Event> Apply(IQueryable<Event> events, EventFilterViewModel filter)
        {
            if (!filter.Date.HasValue)
            {
                return events;
            }

            var selectedDate = filter.Date.Value.Date;

            return events.Where(eventItem => eventItem.EventDate.Date == selectedDate);
        }
    }
}

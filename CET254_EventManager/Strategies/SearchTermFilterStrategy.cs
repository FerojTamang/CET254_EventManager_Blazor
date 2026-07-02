using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Strategies
{
    // Strategy pattern: search text with EF-translatable LIKE expressions.
    public class SearchTermFilterStrategy : IEventFilterStrategy
    {
        public IQueryable<Event> Apply(IQueryable<Event> events, EventFilterViewModel filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                return events;
            }

            var searchTerm = filter.SearchTerm.Trim();
            var pattern = $"%{searchTerm}%";

            return events.Where(eventItem =>
                EF.Functions.Like(eventItem.Title, pattern)
                || EF.Functions.Like(eventItem.Description, pattern));
        }
    }
}

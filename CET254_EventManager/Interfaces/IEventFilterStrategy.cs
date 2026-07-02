using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Interfaces
{
    // Strategy contract for applying event filters polymorphically.
    public interface IEventFilterStrategy
    {
        IQueryable<Event> Apply(IQueryable<Event> events, EventFilterViewModel filter);
    }
}

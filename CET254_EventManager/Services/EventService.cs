using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.EntityFrameworkCore;
using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Services
{
    // Service layer for event queries, validation, and assignments.
    public class EventService : IEventService
    {
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Venue> _venueRepository;
        private readonly IRepository<DomainActivity> _activityRepository;
        private readonly IEnumerable<IEventFilterStrategy> _filterStrategies;

        public EventService(
            IRepository<Event> eventRepository,
            IRepository<Venue> venueRepository,
            IRepository<DomainActivity> activityRepository,
            IEnumerable<IEventFilterStrategy> filterStrategies)
        {
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
            _activityRepository = activityRepository;
            _filterStrategies = filterStrategies;
        }

        public async Task<List<Event>> GetUpcomingEventsAsync()
        {
            // Default upcoming-event query starts from today.
            return await GetUpcomingEventsAsync(DateTime.Today);
        }

        public async Task<List<Event>> GetUpcomingEventsAsync(DateTime fromDate)
        {
            return await _eventRepository.Query()
                .Where(eventItem => eventItem.EventDate >= fromDate.Date)
                .OrderBy(eventItem => eventItem.EventDate)
                .ThenBy(eventItem => eventItem.StartTime)
                .ToListAsync();
        }

        public async Task<List<Event>> GetFilteredUpcomingEventsAsync(EventFilterViewModel filter)
        {
            var query = _eventRepository.Query()
                .Include(e => e.EventVenues)
                    .ThenInclude(eventVenue => eventVenue.Venue)
                .Include(e => e.EventActivities)
                    .ThenInclude(eventActivity => eventActivity.Activity)
                .Where(eventItem => eventItem.EventDate >= DateTime.Today);

            // Strategy pattern: apply each selected filter through the same contract.
            foreach (var strategy in _filterStrategies)
            {
                query = strategy.Apply(query, filter);
            }

            return await query
                .OrderBy(eventItem => eventItem.EventDate)
                .ThenBy(eventItem => eventItem.StartTime)
                .ToListAsync();
        }

        public async Task<Event> GetEventDetailsAsync(int eventId)
        {
            var eventItem = await _eventRepository.Query()
                .Include(e => e.Registrations)
                .Include(e => e.EventVenues)
                    .ThenInclude(eventVenue => eventVenue.Venue)
                .Include(e => e.EventActivities)
                    .ThenInclude(eventActivity => eventActivity.Activity)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            return eventItem ?? throw new EventNotFoundException(eventId);
        }

        public async Task<Event> CreateEventAsync(Event eventItem)
        {
            if (!eventItem.Validate())
            {
                throw new AppException("Event details are invalid.", "INVALID_EVENT");
            }

            await _eventRepository.AddAsync(eventItem);
            await _eventRepository.SaveChangesAsync();

            return eventItem;
        }

        public async Task<Event> CreateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds)
        {
            if (!eventItem.Validate())
            {
                throw new AppException("Event details are invalid.", "INVALID_EVENT");
            }

            var selectedVenueIds = GetDistinctIds(venueIds);
            var selectedActivityIds = GetDistinctIds(activityIds);

            await ValidateVenueIdsAsync(selectedVenueIds);
            await ValidateActivityIdsAsync(selectedActivityIds);

            await _eventRepository.AddAsync(eventItem);
            await _eventRepository.SaveChangesAsync();

            // Add the selected venue and activity links after the event has an Id.
            foreach (var venueId in selectedVenueIds)
            {
                eventItem.EventVenues.Add(new EventVenue(eventItem.Id, venueId));
            }

            foreach (var activityId in selectedActivityIds)
            {
                eventItem.EventActivities.Add(new EventActivity(eventItem.Id, activityId));
            }

            await _eventRepository.SaveChangesAsync();

            return eventItem;
        }

        public async Task UpdateEventAsync(Event eventItem)
        {
            if (!await _eventRepository.ExistsAsync(eventItem.Id))
            {
                throw new EventNotFoundException(eventItem.Id);
            }

            if (!eventItem.Validate())
            {
                throw new AppException("Event details are invalid.", "INVALID_EVENT");
            }

            _eventRepository.Update(eventItem);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event eventItem, IEnumerable<int> venueIds, IEnumerable<int> activityIds)
        {
            var existingEvent = await _eventRepository.Query()
                .Include(e => e.EventVenues)
                .Include(e => e.EventActivities)
                .FirstOrDefaultAsync(e => e.Id == eventItem.Id)
                ?? throw new EventNotFoundException(eventItem.Id);

            if (!eventItem.Validate())
            {
                throw new AppException("Event details are invalid.", "INVALID_EVENT");
            }

            var selectedVenueIds = GetDistinctIds(venueIds);
            var selectedActivityIds = GetDistinctIds(activityIds);

            await ValidateVenueIdsAsync(selectedVenueIds);
            await ValidateActivityIdsAsync(selectedActivityIds);

            existingEvent.UpdateDetails(
                eventItem.Title,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.StartTime,
                eventItem.EndTime);

            ReplaceVenueLinks(existingEvent, selectedVenueIds);
            ReplaceActivityLinks(existingEvent, selectedActivityIds);

            await _eventRepository.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int eventId)
        {
            var eventItem = await _eventRepository.Query()
                .Include(e => e.Registrations)
                .Include(e => e.EventVenues)
                .Include(e => e.EventActivities)
                .FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw new EventNotFoundException(eventId);

            // Delete protection prevents removing events that already have participant registrations.
            if (eventItem.Registrations.Any())
            {
                throw new AppException("This event cannot be deleted because it has registrations.", "EVENT_HAS_REGISTRATIONS");
            }

            eventItem.EventVenues.Clear();
            eventItem.EventActivities.Clear();

            _eventRepository.Delete(eventItem);
            await _eventRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalEventsCountAsync()
        {
            return await _eventRepository.CountAsync();
        }

        private static List<int> GetDistinctIds(IEnumerable<int> ids)
        {
            return ids
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private async Task ValidateVenueIdsAsync(IEnumerable<int> venueIds)
        {
            foreach (var venueId in venueIds)
            {
                if (!await _venueRepository.ExistsAsync(venueId))
                {
                    throw new VenueNotFoundException(venueId);
                }
            }
        }

        private async Task ValidateActivityIdsAsync(IEnumerable<int> activityIds)
        {
            foreach (var activityId in activityIds)
            {
                if (!await _activityRepository.ExistsAsync(activityId))
                {
                    throw new ActivityNotFoundException(activityId);
                }
            }
        }

        private static void ReplaceVenueLinks(Event eventItem, IReadOnlyCollection<int> selectedVenueIds)
        {
            var selectedIds = selectedVenueIds.ToHashSet();

            // Sync venue links by removing unselected links and adding new ones.
            foreach (var existingLink in eventItem.EventVenues
                .Where(eventVenue => !selectedIds.Contains(eventVenue.VenueId))
                .ToList())
            {
                eventItem.EventVenues.Remove(existingLink);
            }

            var existingVenueIds = eventItem.EventVenues
                .Select(eventVenue => eventVenue.VenueId)
                .ToHashSet();

            foreach (var venueId in selectedIds.Where(venueId => !existingVenueIds.Contains(venueId)))
            {
                eventItem.EventVenues.Add(new EventVenue(eventItem.Id, venueId));
            }
        }

        private static void ReplaceActivityLinks(Event eventItem, IReadOnlyCollection<int> selectedActivityIds)
        {
            var selectedIds = selectedActivityIds.ToHashSet();

            foreach (var existingLink in eventItem.EventActivities
                .Where(eventActivity => !selectedIds.Contains(eventActivity.ActivityId))
                .ToList())
            {
                eventItem.EventActivities.Remove(existingLink);
            }

            var existingActivityIds = eventItem.EventActivities
                .Select(eventActivity => eventActivity.ActivityId)
                .ToHashSet();

            foreach (var activityId in selectedIds.Where(activityId => !existingActivityIds.Contains(activityId)))
            {
                eventItem.EventActivities.Add(new EventActivity(eventItem.Id, activityId));
            }
        }
    }
}

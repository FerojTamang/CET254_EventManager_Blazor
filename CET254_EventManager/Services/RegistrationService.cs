using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Services
{
    // Service layer for registration business rules.
    public class RegistrationService : IRegistrationService
    {
        private readonly IRepository<Registration> _registrationRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Participant> _participantRepository;

        public RegistrationService(
            IRepository<Registration> registrationRepository,
            IRepository<Event> eventRepository,
            IRepository<Participant> participantRepository)
        {
            _registrationRepository = registrationRepository;
            _eventRepository = eventRepository;
            _participantRepository = participantRepository;
        }

        public async Task<Registration> RegisterAsync(int eventId, int participantId)
        {
            var eventItem = await _eventRepository.GetByIdAsync(eventId)
                ?? throw new EventNotFoundException(eventId);

            if (!await _participantRepository.ExistsAsync(participantId))
            {
                throw new ParticipantNotFoundException(participantId);
            }

            if (eventItem.EventDate.Date < DateTime.Today)
            {
                throw new PastEventRegistrationException(eventId);
            }

            // Prevent duplicate active bookings, but allow cancelled bookings to reactivate.
            var existingRegistration = await _registrationRepository.Query()
                .FirstOrDefaultAsync(registration => registration.EventId == eventId
                    && registration.ParticipantId == participantId);

            if (existingRegistration is not null && existingRegistration.Status != RegistrationStatus.Cancelled)
            {
                throw new DuplicateRegistrationException($"You have already registered for {eventItem.Title}.");
            }

            await EnsureVenueCapacityAvailableAsync(eventId);

            if (existingRegistration is not null)
            {
                existingRegistration.Reactivate();
                await _registrationRepository.SaveChangesAsync();

                return existingRegistration;
            }

            var registration = new Registration(eventId, participantId);
            await _registrationRepository.AddAsync(registration);
            await _registrationRepository.SaveChangesAsync();

            return registration;
        }

        public async Task CancelRegistrationAsync(int registrationId, int participantId)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId)
                ?? throw new RegistrationNotFoundException(registrationId);

            if (registration.ParticipantId != participantId)
            {
                throw new AppException("This participant is not authorised to access the registration.", "UNAUTHORISED_REGISTRATION_ACCESS");
            }

            registration.Cancel();
            await _registrationRepository.SaveChangesAsync();
        }

        public async Task CancelRegistrationByAdminAsync(int registrationId)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId)
                ?? throw new RegistrationNotFoundException(registrationId);

            registration.Cancel();
            await _registrationRepository.SaveChangesAsync();
        }

        public async Task ConfirmRegistrationAsync(int registrationId)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId)
                ?? throw new RegistrationNotFoundException(registrationId);

            if (registration.Status == RegistrationStatus.Confirmed)
            {
                return;
            }

            if (registration.Status == RegistrationStatus.Cancelled)
            {
                throw new InvalidRegistrationStatusException("Cancelled registrations cannot be confirmed.");
            }

            // Confirming a pending registration reuses the same capacity rule as first-time registration.
            await EnsureVenueCapacityAvailableAsync(registration.EventId, registration.Id);

            registration.Confirm();
            await _registrationRepository.SaveChangesAsync();
        }

        public async Task<Registration> GetRegistrationAsync(int registrationId)
        {
            var registration = await _registrationRepository.Query()
                .Include(r => r.Event)
                .Include(r => r.Participant)
                .FirstOrDefaultAsync(r => r.Id == registrationId);

            return registration ?? throw new RegistrationNotFoundException(registrationId);
        }

        public async Task<List<Registration>> GetParticipantRegistrationsAsync(int participantId)
        {
            return await _registrationRepository.Query()
                .Include(r => r.Event)
                .Where(r => r.ParticipantId == participantId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();
        }

        public async Task<List<Registration>> GetEventRegistrationsAsync(int eventId)
        {
            return await _registrationRepository.Query()
                .Include(r => r.Participant)
                .Where(r => r.EventId == eventId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToListAsync();
        }

        public async Task<List<Registration>> GetRecentRegistrationsAsync(int count)
        {
            return await _registrationRepository.Query()
                .Include(r => r.Event)
                .Include(r => r.Participant)
                .OrderByDescending(r => r.RegistrationDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Dictionary<RegistrationStatus, int>> GetRegistrationSummaryAsync()
        {
            var groupedCounts = await _registrationRepository.Query()
                .GroupBy(r => r.Status)
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToListAsync();

            // Include every status value, even when no registrations currently use it.
            var summary = Enum.GetValues<RegistrationStatus>()
                .ToDictionary(status => status, _ => 0);

            foreach (var groupedCount in groupedCounts)
            {
                summary[groupedCount.Status] = groupedCount.Count;
            }

            return summary;
        }

        private async Task EnsureVenueCapacityAvailableAsync(int eventId, int? excludedRegistrationId = null)
        {
            var eventItem = await _eventRepository.Query()
                .Include(e => e.EventVenues)
                    .ThenInclude(eventVenue => eventVenue.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId)
                ?? throw new EventNotFoundException(eventId);

            if (!eventItem.EventVenues.Any())
            {
                return;
            }

            // Capacity check uses the smallest attached venue as the effective capacity.
            var effectiveCapacity = eventItem.EventVenues
                .Where(eventVenue => eventVenue.Venue is not null)
                .Min(eventVenue => eventVenue.Venue.Capacity);

            var confirmedRegistrations = await _registrationRepository.Query()
                .CountAsync(registration => registration.EventId == eventId
                    && (!excludedRegistrationId.HasValue || registration.Id != excludedRegistrationId.Value)
                    && registration.Status == RegistrationStatus.Confirmed);

            if (confirmedRegistrations >= effectiveCapacity)
            {
                throw new VenueCapacityExceededException(eventId);
            }
        }
    }
}

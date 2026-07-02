using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Services
{
    // Dashboard service composes existing service contracts instead of querying DbContext directly.
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IEventService _eventService;
        private readonly IVenueService _venueService;
        private readonly IActivityService _activityService;
        private readonly IParticipantService _participantService;
        private readonly IRegistrationService _registrationService;

        public AdminDashboardService(
            IEventService eventService,
            IVenueService venueService,
            IActivityService activityService,
            IParticipantService participantService,
            IRegistrationService registrationService)
        {
            _eventService = eventService;
            _venueService = venueService;
            _activityService = activityService;
            _participantService = participantService;
            _registrationService = registrationService;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var statusSummary = await _registrationService.GetRegistrationSummaryAsync();
            var recentRegistrations = await _registrationService.GetRecentRegistrationsAsync(5);

            return new AdminDashboardViewModel
            {
                TotalEvents = await _eventService.GetTotalEventsCountAsync(),
                TotalParticipants = await _participantService.GetTotalParticipantsCountAsync(),
                TotalVenues = await _venueService.GetTotalVenuesCountAsync(),
                TotalActivities = await _activityService.GetTotalActivitiesCountAsync(),
                PendingRegistrations = statusSummary[RegistrationStatus.Pending],
                ConfirmedRegistrations = statusSummary[RegistrationStatus.Confirmed],
                CancelledRegistrations = statusSummary[RegistrationStatus.Cancelled],
                RecentRegistrations = recentRegistrations.Select(MapRegistration).ToList()
            };
        }

        private static RegistrationViewModel MapRegistration(Registration registration)
        {
            return new RegistrationViewModel
            {
                Id = registration.Id,
                EventId = registration.EventId,
                ParticipantId = registration.ParticipantId,
                EventTitle = registration.Event?.Title ?? string.Empty,
                ParticipantName = registration.Participant?.FullName ?? string.Empty,
                RegistrationDate = registration.RegistrationDate,
                Status = registration.Status
            };
        }
    }
}

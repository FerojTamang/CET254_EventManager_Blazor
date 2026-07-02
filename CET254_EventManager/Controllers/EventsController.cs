using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    // Coordinate event HTTP requests and delegate business rules to services.
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IVenueService _venueService;
        private readonly IActivityService _activityService;
        private readonly IParticipantService _participantService;
        private readonly IRegistrationService _registrationService;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(
            IEventService eventService,
            IVenueService venueService,
            IActivityService activityService,
            IParticipantService participantService,
            IRegistrationService registrationService,
            UserManager<IdentityUser> userManager)
        {
            _eventService = eventService;
            _venueService = venueService;
            _activityService = activityService;
            _participantService = participantService;
            _registrationService = registrationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(EventFilterViewModel filter)
        {
            var events = await _eventService.GetFilteredUpcomingEventsAsync(filter);
            var venues = await _venueService.GetAllVenuesAsync();
            var activities = await _activityService.GetAllActivitiesAsync();

            var model = new EventListViewModel
            {
                Filter = filter,
                Events = events.Select(MapEventDetails).ToList(),
                Venues = venues.Select(MapVenue).ToList(),
                Activities = activities.Select(MapActivity).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _eventService.GetEventDetailsAsync(id);
            return View(MapEventDetails(eventItem));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null)
                {
                    return Challenge();
                }

                var participant = await GetOrCreateParticipantAsync(user);
                await _registrationService.RegisterAsync(eventId, participant.Id);
                TempData["Success"] = "Registration submitted successfully.";
                return RedirectToAction("Index", "MyRegistrations");
            }
            catch (AppException ex)
            {
                // Show the registration error on the event details page.
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
        }

        private async Task<Participant> GetOrCreateParticipantAsync(IdentityUser user)
        {
            var applicationUserId = user.Id;
            var participant = await _participantService.GetParticipantByApplicationUserIdAsync(applicationUserId);

            if (participant is not null)
            {
                return participant;
            }

            var email = user.Email ?? user.UserName ?? "user@eventmanager.local";
            var fullName = email.Contains('@') ? email.Split('@')[0] : email;
            var newParticipant = new Participant(fullName, email, string.Empty, applicationUserId);

            return await _participantService.CreateParticipantAsync(newParticipant);
        }

        private static EventDetailsViewModel MapEventDetails(Event eventItem)
        {
            var confirmedCount = eventItem.Registrations.Count(registration => registration.Status == RegistrationStatus.Confirmed);
            var capacities = eventItem.EventVenues
                .Where(eventVenue => eventVenue.Venue is not null)
                .Select(eventVenue => eventVenue.Venue.Capacity)
                .ToList();

            return new EventDetailsViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                VenueNames = eventItem.EventVenues.Select(eventVenue => eventVenue.Venue.Name).ToList(),
                ActivityNames = eventItem.EventActivities.Select(eventActivity => eventActivity.Activity.Name).ToList(),
                ConfirmedRegistrationCount = confirmedCount,
                CapacityRemaining = capacities.Any() ? capacities.Min() - confirmedCount : null,
                IsRegistrationOpen = eventItem.EventDate.Date >= DateTime.Today
            };
        }

        private static VenueViewModel MapVenue(Venue venue)
        {
            return new VenueViewModel
            {
                Id = venue.Id,
                Name = venue.Name,
                Address = venue.Address,
                Capacity = venue.Capacity
            };
        }

        private static ActivityViewModel MapActivity(CET254_EventManager.Models.Activity activity)
        {
            return new ActivityViewModel
            {
                Id = activity.Id,
                Name = activity.Name,
                ActivityType = activity.ActivityType,
                Description = activity.Description
            };
        }
    }
}

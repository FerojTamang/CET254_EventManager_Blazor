using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IVenueService _venueService;
        private readonly IActivityService _activityService;

        public AdminEventsController(
            IEventService eventService,
            IVenueService venueService,
            IActivityService activityService)
        {
            _eventService = eventService;
            _venueService = venueService;
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _eventService.GetEventDetailsAsync(id);
            return View(MapDetails(eventItem));
        }

        public async Task<IActionResult> Create()
        {
            await LoadAssignmentOptionsAsync();
            return View(new EventCreateViewModel
            {
                EventDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(17)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAssignmentOptionsAsync();
                return View(model);
            }

            try
            {
                var eventItem = new Event(model.Title, model.Description, model.EventDate, model.StartTime, model.EndTime);
                await _eventService.CreateEventAsync(eventItem, model.SelectedVenueIds, model.SelectedActivityIds);
                TempData["Success"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadAssignmentOptionsAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var eventItem = await _eventService.GetEventDetailsAsync(id);
            var model = new EventEditViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                EventDate = eventItem.EventDate,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                SelectedVenueIds = eventItem.EventVenues.Select(eventVenue => eventVenue.VenueId).ToList(),
                SelectedActivityIds = eventItem.EventActivities.Select(eventActivity => eventActivity.ActivityId).ToList()
            };

            await LoadAssignmentOptionsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadAssignmentOptionsAsync();
                return View(model);
            }

            try
            {
                var eventItem = await _eventService.GetEventDetailsAsync(id);
                eventItem.UpdateDetails(model.Title, model.Description, model.EventDate, model.StartTime, model.EndTime);
                await _eventService.UpdateEventAsync(eventItem, model.SelectedVenueIds, model.SelectedActivityIds);
                TempData["Success"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadAssignmentOptionsAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _eventService.GetEventDetailsAsync(id);
            return View(MapDetails(eventItem));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id);
                TempData["Success"] = "Event deleted successfully.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadAssignmentOptionsAsync()
        {
            ViewBag.Venues = await _venueService.GetAllVenuesAsync();
            ViewBag.Activities = await _activityService.GetAllActivitiesAsync();
        }

        private static EventDetailsViewModel MapDetails(Event eventItem)
        {
            var confirmedCount = eventItem.Registrations.Count(registration => registration.Status == RegistrationStatus.Confirmed);
            var venueCapacities = eventItem.EventVenues
                .Where(eventVenue => eventVenue.Venue is not null)
                .Select(eventVenue => eventVenue.Venue.Capacity)
                .ToList();
            var capacityRemaining = venueCapacities.Any() ? venueCapacities.Min() - confirmedCount : (int?)null;

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
                CapacityRemaining = capacityRemaining,
                IsRegistrationOpen = eventItem.EventDate.Date >= DateTime.Today
            };
        }
    }
}

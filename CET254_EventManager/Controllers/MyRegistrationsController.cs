using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize]
    public class MyRegistrationsController : Controller
    {
        private readonly IParticipantService _participantService;
        private readonly IRegistrationService _registrationService;
        private readonly UserManager<IdentityUser> _userManager;

        public MyRegistrationsController(
            IParticipantService participantService,
            IRegistrationService registrationService,
            UserManager<IdentityUser> userManager)
        {
            _participantService = participantService;
            _registrationService = registrationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var participant = await GetCurrentParticipantAsync();

            if (participant is null)
            {
                return View(new List<RegistrationViewModel>());
            }

            var registrations = await _registrationService.GetParticipantRegistrationsAsync(participant.Id);
            var model = registrations.Select(registration => MapRegistration(registration, participant.FullName)).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int registrationId)
        {
            try
            {
                var participant = await GetCurrentParticipantAsync();
                if (participant is null)
                {
                    TempData["Error"] = "No participant profile is linked to your account.";
                    return RedirectToAction(nameof(Index));
                }

                await _registrationService.CancelRegistrationAsync(registrationId, participant.Id);
                TempData["Success"] = "Registration cancelled.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Participant?> GetCurrentParticipantAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user is null
                ? null
                : await _participantService.GetParticipantByApplicationUserIdAsync(user.Id);
        }

        private static RegistrationViewModel MapRegistration(Registration registration, string participantName)
        {
            return new RegistrationViewModel
            {
                Id = registration.Id,
                EventId = registration.EventId,
                ParticipantId = registration.ParticipantId,
                EventTitle = registration.Event?.Title ?? string.Empty,
                ParticipantName = participantName,
                RegistrationDate = registration.RegistrationDate,
                Status = registration.Status
            };
        }
    }
}

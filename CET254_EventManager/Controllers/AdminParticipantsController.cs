using CET254_EventManager.Interfaces;
using CET254_EventManager.Exceptions;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize(Roles = "Admin")]
    // Keep participant rules in ParticipantService while the controller handles form flow.
    public class AdminParticipantsController : Controller
    {
        private readonly IParticipantService _participantService;
        private readonly IRegistrationService _registrationService;

        public AdminParticipantsController(
            IParticipantService participantService,
            IRegistrationService registrationService)
        {
            _participantService = participantService;
            _registrationService = registrationService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _participantService.GetAllParticipantsAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            ViewBag.RegistrationCount = (await _registrationService.GetParticipantRegistrationsAsync(id)).Count;
            return View(participant);
        }

        public IActionResult Create()
        {
            return View(new ParticipantViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParticipantViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var participant = new Participant(model.FullName, model.Email, model.PhoneNumber, model.ApplicationUserId);
                await _participantService.CreateParticipantAsync(participant);
                TempData["Success"] = "Participant created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (AppException ex)
            {
                // Show service-layer validation errors on the form.
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            return View(MapParticipant(participant));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ParticipantViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var participant = await _participantService.GetParticipantByIdAsync(id);
                participant.UpdateDetails(model.FullName, model.Email, model.PhoneNumber, model.ApplicationUserId);
                await _participantService.UpdateParticipantAsync(participant);
                TempData["Success"] = "Participant updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var participant = await _participantService.GetParticipantByIdAsync(id);
            ViewBag.RegistrationCount = (await _registrationService.GetParticipantRegistrationsAsync(id)).Count;
            return View(MapParticipant(participant));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _participantService.DeleteParticipantAsync(id);
                TempData["Success"] = "Participant deleted successfully.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static ParticipantViewModel MapParticipant(Participant participant)
        {
            return new ParticipantViewModel
            {
                Id = participant.Id,
                FullName = participant.FullName,
                Email = participant.Email,
                PhoneNumber = participant.PhoneNumber,
                ApplicationUserId = participant.ApplicationUserId
            };
        }
    }
}

using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize(Roles = "Admin")]
    // Delegate admin registration status changes to RegistrationService.
    public class AdminRegistrationsController : Controller
    {
        private readonly IRegistrationService _registrationService;

        public AdminRegistrationsController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        public async Task<IActionResult> Index()
        {
            var registrations = await _registrationService.GetRecentRegistrationsAsync(100);
            return View(registrations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int registrationId)
        {
            try
            {
                await _registrationService.ConfirmRegistrationAsync(registrationId);
                TempData["Success"] = "Registration confirmed.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int registrationId)
        {
            try
            {
                await _registrationService.CancelRegistrationByAdminAsync(registrationId);
                TempData["Success"] = "Registration cancelled.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVenuesController : Controller
    {
        private readonly IVenueService _venueService;

        public AdminVenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _venueService.GetAllVenuesAsync());
        }

        public IActionResult Create()
        {
            return View(new VenueViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _venueService.CreateVenueAsync(new Venue(model.Name, model.Address, model.Capacity));
                TempData["Success"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var venue = await _venueService.GetVenueByIdAsync(id);
            return View(new VenueViewModel { Id = venue.Id, Name = venue.Name, Address = venue.Address, Capacity = venue.Capacity });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueViewModel model)
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
                var venue = await _venueService.GetVenueByIdAsync(id);
                venue.UpdateDetails(model.Name, model.Address, model.Capacity);
                await _venueService.UpdateVenueAsync(venue);
                TempData["Success"] = "Venue updated successfully.";
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
            var venue = await _venueService.GetVenueByIdAsync(id);
            return View(new VenueViewModel { Id = venue.Id, Name = venue.Name, Address = venue.Address, Capacity = venue.Capacity });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _venueService.DeleteVenueAsync(id);
                TempData["Success"] = "Venue deleted successfully.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

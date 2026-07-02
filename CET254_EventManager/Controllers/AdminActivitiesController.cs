using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using CET254_EventManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CET254_EventManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminActivitiesController : Controller
    {
        private readonly IActivityService _activityService;

        public AdminActivitiesController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _activityService.GetAllActivitiesAsync());
        }

        public IActionResult Create()
        {
            return View(new ActivityViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var activity = new CET254_EventManager.Models.Activity(model.Name, model.ActivityType, model.Description);
                await _activityService.CreateActivityAsync(activity);
                TempData["Success"] = "Activity created successfully.";
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
            var activity = await _activityService.GetActivityByIdAsync(id);
            return View(new ActivityViewModel
            {
                Id = activity.Id,
                Name = activity.Name,
                ActivityType = activity.ActivityType,
                Description = activity.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityViewModel model)
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
                var activity = await _activityService.GetActivityByIdAsync(id);
                activity.UpdateDetails(model.Name, model.ActivityType, model.Description);
                await _activityService.UpdateActivityAsync(activity);
                TempData["Success"] = "Activity updated successfully.";
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
            var activity = await _activityService.GetActivityByIdAsync(id);
            return View(new ActivityViewModel
            {
                Id = activity.Id,
                Name = activity.Name,
                ActivityType = activity.ActivityType,
                Description = activity.Description
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _activityService.DeleteActivityAsync(id);
                TempData["Success"] = "Activity deleted successfully.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

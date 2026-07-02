using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Services
{
    // Service layer for activity validation and delete protection.
    public class ActivityService : IActivityService
    {
        private readonly IRepository<DomainActivity> _activityRepository;

        public ActivityService(IRepository<DomainActivity> activityRepository)
        {
            _activityRepository = activityRepository;
        }

        public async Task<List<DomainActivity>> GetAllActivitiesAsync()
        {
            return await _activityRepository.Query()
                .OrderBy(activity => activity.Name)
                .ToListAsync();
        }

        public async Task<DomainActivity> GetActivityByIdAsync(int activityId)
        {
            return await _activityRepository.GetByIdAsync(activityId)
                ?? throw new ActivityNotFoundException(activityId);
        }

        public async Task<DomainActivity> CreateActivityAsync(DomainActivity activity)
        {
            if (!activity.Validate())
            {
                throw new AppException("Activity details are invalid.", "INVALID_ACTIVITY");
            }

            await _activityRepository.AddAsync(activity);
            await _activityRepository.SaveChangesAsync();

            return activity;
        }

        public async Task UpdateActivityAsync(DomainActivity activity)
        {
            if (!await _activityRepository.ExistsAsync(activity.Id))
            {
                throw new ActivityNotFoundException(activity.Id);
            }

            if (!activity.Validate())
            {
                throw new AppException("Activity details are invalid.", "INVALID_ACTIVITY");
            }

            _activityRepository.Update(activity);
            await _activityRepository.SaveChangesAsync();
        }

        public async Task DeleteActivityAsync(int activityId)
        {
            var activity = await _activityRepository.Query()
                .Include(activity => activity.EventActivities)
                .FirstOrDefaultAsync(activity => activity.Id == activityId)
                ?? throw new ActivityNotFoundException(activityId);

            // Delete protection: activities assigned to events cannot be removed.
            if (activity.EventActivities.Any())
            {
                throw new AppException("This activity cannot be deleted because it is assigned to one or more events.", "ACTIVITY_IN_USE");
            }

            _activityRepository.Delete(activity);
            await _activityRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalActivitiesCountAsync()
        {
            return await _activityRepository.CountAsync();
        }
    }
}

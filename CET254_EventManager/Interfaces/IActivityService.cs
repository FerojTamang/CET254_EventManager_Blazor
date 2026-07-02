using DomainActivity = CET254_EventManager.Models.Activity;

namespace CET254_EventManager.Interfaces
{
    // Service contract for activity management operations.
    public interface IActivityService
    {
        Task<List<DomainActivity>> GetAllActivitiesAsync();

        Task<DomainActivity> GetActivityByIdAsync(int activityId);

        Task<DomainActivity> CreateActivityAsync(DomainActivity activity);

        Task UpdateActivityAsync(DomainActivity activity);

        Task DeleteActivityAsync(int activityId);

        Task<int> GetTotalActivitiesCountAsync();
    }
}

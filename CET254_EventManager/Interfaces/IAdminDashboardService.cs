using CET254_EventManager.ViewModels;

namespace CET254_EventManager.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}

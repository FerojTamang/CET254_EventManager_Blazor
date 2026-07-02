using CET254_EventManager.Models;

namespace CET254_EventManager.Interfaces
{
    // Service contract for venue management operations.
    public interface IVenueService
    {
        Task<List<Venue>> GetAllVenuesAsync();

        Task<Venue> GetVenueByIdAsync(int venueId);

        Task<Venue> CreateVenueAsync(Venue venue);

        Task UpdateVenueAsync(Venue venue);

        Task DeleteVenueAsync(int venueId);

        Task<int> GetTotalVenuesCountAsync();
    }
}

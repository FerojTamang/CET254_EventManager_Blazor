using CET254_EventManager.Models;

namespace CET254_EventManager.Interfaces
{
    // Service contract for registration use cases.
    public interface IRegistrationService
    {
        Task<Registration> RegisterAsync(int eventId, int participantId);

        Task CancelRegistrationAsync(int registrationId, int participantId);

        Task CancelRegistrationByAdminAsync(int registrationId);

        Task ConfirmRegistrationAsync(int registrationId);

        Task<Registration> GetRegistrationAsync(int registrationId);

        Task<List<Registration>> GetParticipantRegistrationsAsync(int participantId);

        Task<List<Registration>> GetEventRegistrationsAsync(int eventId);

        Task<List<Registration>> GetRecentRegistrationsAsync(int count);

        Task<Dictionary<RegistrationStatus, int>> GetRegistrationSummaryAsync();
    }
}

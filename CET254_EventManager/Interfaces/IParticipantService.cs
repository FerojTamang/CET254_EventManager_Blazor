using CET254_EventManager.Models;

namespace CET254_EventManager.Interfaces
{
    // Service contract for participant management.
    public interface IParticipantService
    {
        Task<List<Participant>> GetAllParticipantsAsync();

        Task<Participant> GetParticipantByIdAsync(int participantId);

        Task<Participant?> GetParticipantByApplicationUserIdAsync(string applicationUserId);

        Task<Participant?> GetParticipantByEmailAsync(string email);

        Task<Participant> CreateParticipantAsync(Participant participant);

        Task UpdateParticipantAsync(Participant participant);

        Task DeleteParticipantAsync(int participantId);

        Task<int> GetTotalParticipantsCountAsync();
    }
}

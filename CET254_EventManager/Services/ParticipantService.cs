using System.ComponentModel.DataAnnotations;
using CET254_EventManager.Exceptions;
using CET254_EventManager.Interfaces;
using CET254_EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Services
{
    // Service layer for participant validation and safe delete rules.
    public class ParticipantService : IParticipantService
    {
        private readonly IRepository<Participant> _participantRepository;

        public ParticipantService(IRepository<Participant> participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task<List<Participant>> GetAllParticipantsAsync()
        {
            return await _participantRepository.Query()
                .OrderBy(participant => participant.FullName)
                .ToListAsync();
        }

        public async Task<Participant> GetParticipantByIdAsync(int participantId)
        {
            return await _participantRepository.GetByIdAsync(participantId)
                ?? throw new ParticipantNotFoundException(participantId);
        }

        public async Task<Participant?> GetParticipantByApplicationUserIdAsync(string applicationUserId)
        {
            return await _participantRepository.Query()
                .FirstOrDefaultAsync(participant => participant.ApplicationUserId == applicationUserId);
        }

        public async Task<Participant?> GetParticipantByEmailAsync(string email)
        {
            var normalisedEmail = NormaliseEmail(email);

            return await _participantRepository.Query()
                .FirstOrDefaultAsync(participant => participant.Email.ToUpper() == normalisedEmail);
        }

        public async Task<Participant> CreateParticipantAsync(Participant participant)
        {
            ValidateParticipant(participant);

            var normalisedEmail = NormaliseEmail(participant.Email);

            // Enforce participant email uniqueness case-insensitively.
            var duplicateEmailExists = await _participantRepository.Query()
                .AnyAsync(existing => existing.Email.ToUpper() == normalisedEmail);

            if (duplicateEmailExists)
            {
                throw new AppException("A participant with this email already exists.", "DUPLICATE_PARTICIPANT_EMAIL");
            }

            await _participantRepository.AddAsync(participant);
            await _participantRepository.SaveChangesAsync();

            return participant;
        }

        public async Task UpdateParticipantAsync(Participant participant)
        {
            if (!await _participantRepository.ExistsAsync(participant.Id))
            {
                throw new ParticipantNotFoundException(participant.Id);
            }

            ValidateParticipant(participant);

            var normalisedEmail = NormaliseEmail(participant.Email);

            var duplicateEmailExists = await _participantRepository.Query()
                .AnyAsync(existing => existing.Email.ToUpper() == normalisedEmail
                    && existing.Id != participant.Id);

            if (duplicateEmailExists)
            {
                throw new AppException("A participant with this email already exists.", "DUPLICATE_PARTICIPANT_EMAIL");
            }

            _participantRepository.Update(participant);
            await _participantRepository.SaveChangesAsync();
        }

        public async Task DeleteParticipantAsync(int participantId)
        {
            var participant = await _participantRepository.Query()
                .Include(participant => participant.Registrations)
                .FirstOrDefaultAsync(participant => participant.Id == participantId)
                ?? throw new ParticipantNotFoundException(participantId);

            // Safe delete: keep participants that still have registration history.
            if (participant.Registrations.Any())
            {
                throw new AppException("This participant cannot be deleted because they have event registrations.", "PARTICIPANT_HAS_REGISTRATIONS");
            }

            _participantRepository.Delete(participant);
            await _participantRepository.SaveChangesAsync();
        }

        public async Task<int> GetTotalParticipantsCountAsync()
        {
            return await _participantRepository.CountAsync();
        }

        private static void ValidateParticipant(Participant participant)
        {
            if (!participant.Validate()
                || participant.FullName.Length > 150
                || participant.Email.Length > 256
                || participant.PhoneNumber.Length > 30
                || participant.ApplicationUserId?.Length > 450)
            {
                throw new AppException("Participant details are invalid.", "INVALID_PARTICIPANT");
            }

            if (!new EmailAddressAttribute().IsValid(participant.Email))
            {
                throw new AppException("Enter a valid participant email address.", "INVALID_PARTICIPANT_EMAIL");
            }

            if (!string.IsNullOrWhiteSpace(participant.PhoneNumber)
                && !new PhoneAttribute().IsValid(participant.PhoneNumber))
            {
                throw new AppException("Enter a valid participant phone number.", "INVALID_PARTICIPANT_PHONE");
            }
        }

        private static string NormaliseEmail(string email)
        {
            return email.Trim().ToUpper();
        }
    }
}

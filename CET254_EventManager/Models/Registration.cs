using CET254_EventManager.Exceptions;

namespace CET254_EventManager.Models
{
    // Event-participant booking with registration date and status.
    public class Registration : BaseEntity
    {
        protected Registration()
        {
        }

        // New registrations start pending with a timestamp.
        public Registration(int eventId, int participantId)
        {
            EventId = eventId;
            ParticipantId = participantId;
            RegistrationDate = DateTime.UtcNow;
            Status = RegistrationStatus.Pending;
        }

        public int EventId { get; private set; }

        public int ParticipantId { get; private set; }

        public DateTime RegistrationDate { get; private set; }

        public RegistrationStatus Status { get; private set; }

        public Event Event { get; private set; } = null!;

        public Participant Participant { get; private set; } = null!;

        // Keep status transitions inside the registration entity.
        public void Confirm()
        {
            Status = RegistrationStatus.Confirmed;
            Touch();
        }

        public void Cancel()
        {
            Status = RegistrationStatus.Cancelled;
            Touch();
        }

        public void Reactivate()
        {
            if (Status != RegistrationStatus.Cancelled)
            {
                throw new InvalidRegistrationStatusException("Only cancelled registrations can be reactivated.");
            }

            Status = RegistrationStatus.Pending;
            RegistrationDate = DateTime.UtcNow;
            Touch();
        }

        public override bool Validate()
        {
            return EventId > 0
                && ParticipantId > 0;
        }
    }
}

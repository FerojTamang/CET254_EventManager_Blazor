namespace CET254_EventManager.Models
{
    // Encapsulation: update participant profile data through methods.
    public class Participant : BaseEntity
    {
        protected Participant()
        {
        }

        // Create a participant profile and optionally link an Identity user.
        public Participant(string fullName, string email, string phoneNumber, string? applicationUserId = null)
        {
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            ApplicationUserId = applicationUserId;
        }

        public string FullName { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string PhoneNumber { get; private set; } = string.Empty;

        public string? ApplicationUserId { get; private set; }

        public ICollection<Registration> Registrations { get; private set; } = new List<Registration>();

        public void UpdateDetails(string fullName, string email, string phoneNumber, string? applicationUserId = null)
        {
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            ApplicationUserId = applicationUserId;

            Touch();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(FullName)
                && !string.IsNullOrWhiteSpace(Email);
        }
    }
}

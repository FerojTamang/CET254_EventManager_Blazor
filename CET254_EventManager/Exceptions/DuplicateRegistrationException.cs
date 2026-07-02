namespace CET254_EventManager.Exceptions
{
    // Use a stable error code for duplicate registration failures.
    public class DuplicateRegistrationException : AppException
    {
        public DuplicateRegistrationException(int eventId, int participantId)
            : base("You have already registered for this event.", "DUPLICATE_REGISTRATION")
        {
        }

        public DuplicateRegistrationException(string message)
            : base(message, "DUPLICATE_REGISTRATION")
        {
        }
    }
}

namespace CET254_EventManager.Exceptions
{
    // Block registrations for events that have already passed.
    public class PastEventRegistrationException : AppException
    {
        public PastEventRegistrationException(int eventId)
            : base("Registration is not allowed because this event is in the past.", "PAST_EVENT_REGISTRATION")
        {
        }
    }
}

namespace CET254_EventManager.Exceptions
{
    // Use a domain exception for invalid registration status transitions.
    public class InvalidRegistrationStatusException : AppException
    {
        public InvalidRegistrationStatusException(string message)
            : base(message, "INVALID_REGISTRATION_STATUS")
        {
        }
    }
}

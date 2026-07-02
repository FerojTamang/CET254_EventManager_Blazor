namespace CET254_EventManager.Exceptions
{
    // Use a domain exception for registration lookup failures.
    public class RegistrationNotFoundException : AppException
    {
        public RegistrationNotFoundException(int registrationId)
            : base("The requested registration was not found.", "REGISTRATION_NOT_FOUND")
        {
        }
    }
}

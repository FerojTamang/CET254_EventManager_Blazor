namespace CET254_EventManager.Exceptions
{
    // Use a domain exception when an event cannot be found.
    public class EventNotFoundException : AppException
    {
        public EventNotFoundException(int eventId)
            : base("The requested event was not found.", "EVENT_NOT_FOUND")
        {
        }
    }
}

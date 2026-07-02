namespace CET254_EventManager.Exceptions
{
    // Use a domain exception when event capacity is full.
    public class VenueCapacityExceededException : AppException
    {
        public VenueCapacityExceededException(int eventId)
            : base("The venue capacity has been reached for this event.", "VENUE_CAPACITY_EXCEEDED")
        {
        }
    }
}

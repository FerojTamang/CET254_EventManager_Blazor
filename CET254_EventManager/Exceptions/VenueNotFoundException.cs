namespace CET254_EventManager.Exceptions
{
    // Use a domain exception for venue lookup failures.
    public class VenueNotFoundException : AppException
    {
        public VenueNotFoundException(int venueId)
            : base("The requested venue was not found.", "VENUE_NOT_FOUND")
        {
        }
    }
}

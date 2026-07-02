namespace CET254_EventManager.Models
{
    // Junction entity for the Event-Venue many-to-many relationship.
    public class EventVenue
    {
        protected EventVenue()
        {
        }

        public EventVenue(int eventId, int venueId)
        {
            EventId = eventId;
            VenueId = venueId;
        }

        public int EventId { get; private set; }

        public int VenueId { get; private set; }

        public Event Event { get; private set; } = null!;

        public Venue Venue { get; private set; } = null!;
    }
}

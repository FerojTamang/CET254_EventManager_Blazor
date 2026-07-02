namespace CET254_EventManager.Models
{
    // Junction entity for the Event-Activity many-to-many relationship.
    public class EventActivity
    {
        protected EventActivity()
        {
        }

        public EventActivity(int eventId, int activityId)
        {
            EventId = eventId;
            ActivityId = activityId;
        }

        public int EventId { get; private set; }

        public int ActivityId { get; private set; }

        public Event Event { get; private set; } = null!;

        public Activity Activity { get; private set; } = null!;
    }
}

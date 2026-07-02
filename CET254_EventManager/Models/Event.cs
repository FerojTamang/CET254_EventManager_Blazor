namespace CET254_EventManager.Models
{
    // Encapsulation: update event details through methods, not public setters.
    public class Event : BaseEntity
    {
        protected Event()
        {
        }

        // Initialise the fields required for a valid event.
        public Event(string title, string description, DateTime eventDate, TimeSpan startTime, TimeSpan endTime)
        {
            Title = title;
            Description = description;
            EventDate = eventDate;
            StartTime = startTime;
            EndTime = endTime;
        }

        public string Title { get; private set; } = string.Empty;

        public string Description { get; private set; } = string.Empty;

        public DateTime EventDate { get; private set; }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public ICollection<Registration> Registrations { get; private set; } = new List<Registration>();

        public ICollection<EventVenue> EventVenues { get; private set; } = new List<EventVenue>();

        public ICollection<EventActivity> EventActivities { get; private set; } = new List<EventActivity>();

        public void UpdateDetails(string title, string description, DateTime eventDate, TimeSpan startTime, TimeSpan endTime)
        {
            Title = title;
            Description = description;
            EventDate = eventDate;
            StartTime = startTime;
            EndTime = endTime;

            Touch();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Title)
                && EventDate != default
                && EndTime > StartTime;
        }
    }
}

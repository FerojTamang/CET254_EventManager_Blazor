namespace CET254_EventManager.ViewModels
{
    // ViewModels separate display data from domain entities for MVC screens.
    public class EventDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public List<string> VenueNames { get; set; } = new List<string>();

        public List<string> ActivityNames { get; set; } = new List<string>();

        public int ConfirmedRegistrationCount { get; set; }

        public int? CapacityRemaining { get; set; }

        public bool IsRegistrationOpen { get; set; }
    }
}

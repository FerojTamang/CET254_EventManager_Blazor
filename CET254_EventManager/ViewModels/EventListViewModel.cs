namespace CET254_EventManager.ViewModels
{
    // Supports the event listing and filtering UI without exposing domain entities directly to the view.
    public class EventListViewModel
    {
        public EventFilterViewModel Filter { get; set; } = new EventFilterViewModel();

        public List<EventDetailsViewModel> Events { get; set; } = new List<EventDetailsViewModel>();

        public List<VenueViewModel> Venues { get; set; } = new List<VenueViewModel>();

        public List<ActivityViewModel> Activities { get; set; } = new List<ActivityViewModel>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace CET254_EventManager.ViewModels
{
    public class EventFilterViewModel
    {
        // Carries optional filtering criteria from the UI to the service layer.
        [Display(Name = "Event Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Display(Name = "Venue")]
        public int? VenueId { get; set; }

        [Display(Name = "Activity")]
        public int? ActivityId { get; set; }

        [Display(Name = "Activity Type")]
        public string? ActivityType { get; set; }

        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters.")]
        public string? SearchTerm { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using CET254_EventManager.Validation;

namespace CET254_EventManager.ViewModels
{
    // Data annotations drive ModelState validation before the controller calls the service layer.
    public class EventCreateViewModel
    {
        [Required(ErrorMessage = "Event title is required.")]
        [StringLength(150, ErrorMessage = "Event title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        [FutureDate]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public List<int> SelectedVenueIds { get; set; } = new List<int>();

        public List<int> SelectedActivityIds { get; set; } = new List<int>();
    }
}

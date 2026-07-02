using System.ComponentModel.DataAnnotations;

namespace CET254_EventManager.ViewModels
{
    // ViewModels separate MVC form data from domain entities and support validation.
    public class VenueViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Venue name is required.")]
        [StringLength(150, ErrorMessage = "Venue name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Venue address is required.")]
        [StringLength(300, ErrorMessage = "Venue address cannot exceed 300 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Venue capacity is required.")]
        [Range(1, 100000, ErrorMessage = "Venue capacity must be between 1 and 100000.")]
        public int Capacity { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using CET254_EventManager.Models;

namespace CET254_EventManager.ViewModels
{
    // ViewModels separate MVC form data from domain entities and support validation.
    public class ActivityViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(150, ErrorMessage = "Activity name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activity type is required.")]
        public ActivityType ActivityType { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
    }
}

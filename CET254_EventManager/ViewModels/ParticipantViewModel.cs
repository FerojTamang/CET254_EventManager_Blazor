using System.ComponentModel.DataAnnotations;

namespace CET254_EventManager.ViewModels
{
    // Data annotations validate admin participant forms before domain/service validation runs.
    public class ParticipantViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(450, ErrorMessage = "Application user ID cannot exceed 450 characters.")]
        public string? ApplicationUserId { get; set; }
    }
}

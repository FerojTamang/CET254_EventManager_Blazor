using System.ComponentModel.DataAnnotations;
using CET254_EventManager.Models;

namespace CET254_EventManager.ViewModels
{
    // ViewModels separate display data from domain entities for MVC screens.
    public class RegistrationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Event ID")]
        public int EventId { get; set; }

        [Display(Name = "Participant ID")]
        public int ParticipantId { get; set; }

        [Display(Name = "Event")]
        public string EventTitle { get; set; } = string.Empty;

        [Display(Name = "Participant")]
        public string ParticipantName { get; set; } = string.Empty;

        [Display(Name = "Registration Date")]
        [DataType(DataType.DateTime)]
        public DateTime RegistrationDate { get; set; }

        [Display(Name = "Status")]
        public RegistrationStatus Status { get; set; }
    }
}

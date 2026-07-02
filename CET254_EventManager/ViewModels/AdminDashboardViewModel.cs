namespace CET254_EventManager.ViewModels
{
    // ViewModels keep dashboard display data separate from domain entities.
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }

        public int TotalParticipants { get; set; }

        public int TotalVenues { get; set; }

        public int TotalActivities { get; set; }

        public int PendingRegistrations { get; set; }

        public int ConfirmedRegistrations { get; set; }

        public int CancelledRegistrations { get; set; }

        public List<RegistrationViewModel> RecentRegistrations { get; set; } = new List<RegistrationViewModel>();
    }
}

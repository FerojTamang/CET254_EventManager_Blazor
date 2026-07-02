namespace CET254_EventManager.Models
{
    // Encapsulation: update venue capacity and address through methods.
    public class Venue : BaseEntity
    {
        protected Venue()
        {
        }

        // Initialise the venue details needed for scheduling.
        public Venue(string name, string address, int capacity)
        {
            Name = name;
            Address = address;
            Capacity = capacity;
        }

        public string Name { get; private set; } = string.Empty;

        public string Address { get; private set; } = string.Empty;

        public int Capacity { get; private set; }

        public ICollection<EventVenue> EventVenues { get; private set; } = new List<EventVenue>();

        public void UpdateDetails(string name, string address, int capacity)
        {
            Name = name;
            Address = address;
            Capacity = capacity;

            Touch();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && Capacity > 0;
        }
    }
}

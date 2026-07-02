namespace CET254_EventManager.Models
{
    // Encapsulation: update activity details through methods.
    public class Activity : BaseEntity
    {
        protected Activity()
        {
        }

        // Initialise an activity option that can be assigned to events.
        public Activity(string name, ActivityType activityType, string description)
        {
            Name = name;
            ActivityType = activityType;
            Description = description;
        }

        public string Name { get; private set; } = string.Empty;

        public ActivityType ActivityType { get; private set; }

        public string Description { get; private set; } = string.Empty;

        public ICollection<EventActivity> EventActivities { get; private set; } = new List<EventActivity>();

        public void UpdateDetails(string name, ActivityType activityType, string description)
        {
            Name = name;
            ActivityType = activityType;
            Description = description;

            Touch();
        }

        public override bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}

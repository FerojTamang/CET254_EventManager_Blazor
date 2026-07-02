namespace CET254_EventManager.Exceptions
{
    // Use a domain exception for activity lookup failures.
    public class ActivityNotFoundException : AppException
    {
        public ActivityNotFoundException(int activityId)
            : base("The requested activity was not found.", "ACTIVITY_NOT_FOUND")
        {
        }
    }
}

namespace CET254_EventManager.Exceptions
{
    // Use a domain exception for participant lookup failures.
    public class ParticipantNotFoundException : AppException
    {
        public ParticipantNotFoundException(int participantId)
            : base("The requested participant was not found.", "PARTICIPANT_NOT_FOUND")
        {
        }
    }
}

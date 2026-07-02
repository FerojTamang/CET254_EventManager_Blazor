namespace CET254_EventManager.Exceptions
{
    // Domain errors use safe messages that controllers can show to users.
    public class AppException : Exception
    {
        public AppException()
        {
        }

        public AppException(string message)
            : base(message)
        {
        }

        public AppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AppException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public string? ErrorCode { get; }
    }
}

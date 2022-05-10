namespace SecureDocuments.Models.Common
{
    public record Error
    {
        public Error(string message, Exception? ex = null)
        {
            Msg = message;
            Exception = ex;
        }
        public string Msg { get; init; }
        public Exception? Exception { get; init; }
    }
}
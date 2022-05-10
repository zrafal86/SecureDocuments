namespace SecureDocuments.Models
{
    public record WatchedFile
    {
        public string? Name { get; init; }
        public string? FullPath { get; init; }
        public WatcherChangeTypes ChangeType { get; init; }
        public string? OldName { get; init; }
        public string? OldFullPath { get; init; }
    }
}
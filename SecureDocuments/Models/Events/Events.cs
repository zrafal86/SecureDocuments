namespace SecureDocuments.Models.Events
{
    public record ApplicationFolderIsNotSetEvent { }

    public record LoggedInUserEvent
    {
        public User? User { get; init; }
    }

    public record InvalidUserLoginEvent { }

    public record CannotFindConfigFileEvent
    {
        public string? Message { get; init; }
    }

    public record RefreshActionEvent
    {
        public string? Id { get; init; }
        public bool LoadFromFiles { get; init; }
    }

    public record ShowWaitDialogEvent
    {
        public bool IsVisible { get; init; }
    }

    public record ChangedThemeEvent
    {
        public bool IsDark { get; init; }
    }

    public record ChangedNotificationEvent
    {
        public bool EnabledNotification { get; init; }
    }
}
using SecureDocuments.Models;

namespace SecureDocuments
{
    public interface IUserSettings
    {
        public string? ApplicationFolder { get; set; }
        public string? LastUserEmail { get; set; }
        public bool IsDarkTheme { get; set; }
        public bool EnableNotification { get; set; }

        User? User { get; set; }
    }
}
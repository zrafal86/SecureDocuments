using SecureDocuments.Models;
using SecureDocuments.WPF.Properties;

namespace SecureDocuments.WPF
{
    public class UserSettings : IUserSettings
    {
        public string? ApplicationFolder
        {
            get => Settings.Default.ApplicationFolder;
            set
            {
                Settings.Default.ApplicationFolder = value;
                Settings.Default.Save();
            }
        }

        public string? LastUserEmail
        {
            get => Settings.Default.LastUserEmail;
            set
            {
                Settings.Default.LastUserEmail = value;
                Settings.Default.Save();
            }
        }

        public User? User { get; set; }

        public bool IsDarkTheme
        {
            get => Settings.Default.IsDarkTheme;
            set
            {
                Settings.Default.IsDarkTheme = value;
                Settings.Default.Save();
            }
        }

        public bool EnableNotification
        {
            get => Settings.Default.EnableNotification;
            set
            {
                Settings.Default.EnableNotification = value;
                Settings.Default.Save();
            }
        }
    }
}
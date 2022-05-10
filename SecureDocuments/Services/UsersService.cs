using SecureDocuments.Data;
using SecureDocuments.Models;
using Splat;

namespace SecureDocuments.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>?> GetUsersAsync(Predicate<User>? predicate = null);
        Task<AppConfig?> GetAppConfig();
    }

    public sealed class UserService : IUserService
    {
        private readonly IAppConfigSource? _appConfigSource;
        private readonly IUserSettings _userSettings;

        public UserService(IUserSettings userSettings)
        {
            _appConfigSource = Locator.Current.GetService<IAppConfigSource>();
            _userSettings = userSettings;
        }

        public async Task<AppConfig?> GetAppConfig()
        {
            var file = new FileInfo(_userSettings.ApplicationFolder + Path.DirectorySeparatorChar + ConstantValues.ConfigFile);
            if (file.Exists && _appConfigSource != null)
            {
                return await _appConfigSource.Get(file.FullName);
            }
            return null;
        }

        public async Task<IEnumerable<User>?> GetUsersAsync(Predicate<User>? predicate = null)
        {
            var appFolder = _userSettings.ApplicationFolder;
            var file = new FileInfo(appFolder + Path.DirectorySeparatorChar + ConstantValues.ConfigFile);
            if (file.Exists && _appConfigSource != null)
            {
                var config = await _appConfigSource.Get(file.FullName);
                var allUsers = config?.Users;
                if (predicate != null)
                {
                    allUsers = config?.Users?
                        .Where(user => predicate(user));
                }

                return allUsers?.ToList();
            }

            return Enumerable.Empty<User>();
        }
    }
}
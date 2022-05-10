using SecureDocuments.Data;
using SecureDocuments.Encryption.Hash;
using SecureDocuments.Models;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;

namespace SecureDocuments.ViewModels
{
    public sealed class ProfileViewModel : ViewModelBase
    {
        public ProfileViewModel(IScreen? screen = null) : base(screen)
        {
            IsDark = _userSettings?.IsDarkTheme ?? false;
            UserEmail = _userSettings?.User?.Email;
            EnabledNotification = _userSettings?.EnableNotification ?? false;

            _appConfigStore = Locator.Current.GetService<IAppConfigSource>();

            var canChangedPassword = this.WhenAnyValue(vm => vm.Password, vm => vm.NewPassword, (pass, newPass) =>
            !string.IsNullOrEmpty(pass) && !string.IsNullOrEmpty(newPass) && pass?.Length >= 5 && newPass?.Length >= 5);
            UpdatePassword = ReactiveCommand.CreateFromTask(ChangedPassword, canChangedPassword);
        }

        public override string UrlPathSegment => "Profile";

        [Reactive] public bool IsDark { get; set; }
        [Reactive] public bool EnabledNotification { get; set; }

        private readonly IAppConfigSource? _appConfigStore;

        [Reactive] public string? Password { get; set; }
        [Reactive] public string? UserEmail { get; set; }
        [Reactive] public string? NewPassword { get; set; }
        public ReactiveCommand<Unit, Unit> UpdatePassword { get; set; }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --Profile-- screen!");
            this.WhenAnyValue(vm => vm.IsDark)
                .Subscribe(x => SetIsDark(x))
                .DisposeWith(disposables);
            this.WhenAnyValue(vm => vm.EnabledNotification)
                .Subscribe(x => SetNotification(x))
                .DisposeWith(disposables);
        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --Profile-- screen!");
        }

        private static void SetIsDark(bool isDark)
        {
            MessageBus.Current.SendMessage(new ChangedThemeEvent { IsDark = isDark });
        }

        private static void SetNotification(bool enabled)
        {
            MessageBus.Current.SendMessage(new ChangedNotificationEvent { EnabledNotification = enabled });
        }

        private async Task<Unit> ChangedPassword()
        {
            var (success, user) = await CheckUserPassword();

            var isChanged = false;
            if (success && user != null)
            {
                try
                {
                    var isSaved = await SaveToFile(user!);
                    isChanged = isSaved;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
            }

            await ShowResultPopup(isChanged);

            return await Task.FromResult(Unit.Default);
        }

        private async Task<(bool, User?)> CheckUserPassword()
        {
            return await Task.Run<(bool, User?)>(() =>
            {
                var user = _userSettings?.User;
                if (user != null)
                {
                    var hashed = Hash.DoHash(Password!);
                    if (hashed != null && hashed.Equals(user.PasswordHash)) return (true, user);
                }

                return (false, user);
            });
        }

        private async Task<bool> SaveToFile(User user)
        {
            var hashedPassword = Hash.DoHash(NewPassword!);

            var path = PathFactory.GetAppConfigFile();
            var appConfig = await _appConfigStore!.Get(path);
            var users = appConfig?.Users;
            if (appConfig != null && users != null)
            {
                var usersList = users.ToList();
                var newUser = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PasswordHash = hashedPassword,
                    Role = user.Role,
                    Phone = user.Phone,
                };

                usersList.Remove(user);
                usersList.Add(newUser);
                var newConfig = new AppConfig
                {
                    Users = usersList,
                    EmailNotification = appConfig.EmailNotification,
                    CompanyNames = appConfig.CompanyNames,
                    Subjects = appConfig.Subjects,
                    CustomerCountries = appConfig.CustomerCountries,
                    ListOfPeopleToBeInformedWhenOfferAccepted = appConfig.ListOfPeopleToBeInformedWhenOfferAccepted,
                };
                _userSettings!.User = newUser;
                await _appConfigStore.Save(newConfig, path);
                return true;
            }
            return false;
        }

        private static async Task ShowResultPopup(bool isChanged)
        {
            var dialogService = Locator.Current.GetService<IDialogService>();
            if (dialogService != null)
            {
                if (!isChanged)
                {
                    await dialogService.ShowDialog("Password not changed", "There occurs some problems to change the password.");
                }
                else
                {
                    await dialogService.ShowDialog("Password changed", "You have changed password with success.");
                }
            }
        }
    }
}
using SecureDocuments.Data;
using SecureDocuments.Models;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;

namespace SecureDocuments.ViewModels
{
    public sealed class MainViewModel : ReactiveObject, IScreen, IEnableLogger
    {
        private readonly IUserSettings _userSettings;
        private readonly IAppConfigSource _appConfigSource;
        private readonly ISessionSource _sessionSource;

        public MainViewModel(IMutableDependencyResolver resolver, Action registerViews)
        {
            Router = new RoutingState();

            resolver.RegisterConstant<IScreen>(this);

            registerViews();

            _userSettings = Locator.Current.GetService<IUserSettings>()!;
            _appConfigSource = Locator.Current.GetService<IAppConfigSource>()!;
            _sessionSource = Locator.Current.GetService<ISessionSource>()!;

            SetApplicationFolder();

            Log.Information("#########################################################");
            Log.Information("###################### MAIN #############################");
            Log.Information("#########################################################");

            CanNavigateBack = this.WhenAnyValue(vm => vm.Router.NavigationStack.Count).Select(count => count > 1);

            MessageBus.Current.Listen<LoggedInUserEvent>().Subscribe(eventObject =>
            {
                var user = eventObject.User;
                if (_userSettings != null)
                {
                    _userSettings.User = eventObject.User;
                }
                IsLoggedIn = user != null;
                if (user != null)
                {
                    UserName = user.FullName;
                    UserRole = $"[{user.Role}]";
                }
            });

            MessageBus.Current.Listen<ChangedThemeEvent>().Subscribe(eventObject =>
            {
                var isDark = eventObject.IsDark;
                _userSettings.IsDarkTheme = isDark;
                Locator.Current.GetService<IThemeService>()!.SetTheme(isDark);
            });

            MessageBus.Current.Listen<ChangedNotificationEvent>().Subscribe(eventObject =>
            {
                var enable = eventObject.EnabledNotification;
                _userSettings.EnableNotification = enable;
            });

            LoginScreenCommand = ReactiveCommand.Create(ShowLoginScreen);
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutUser);
            NavigateBackCommand = ReactiveCommand.CreateFromTask(async () => await Router.NavigateBack.Execute(), CanNavigateBack);
            ProfilCommand = ReactiveCommand.Create(ShowProfileScreen);

            var LoginScreenCommand2 = ReactiveCommand.CreateFromTask<Unit, User?>(GetUserFromSession, outputScheduler: RxApp.MainThreadScheduler);
            LoginScreenCommand2.ThrownExceptions
                .Subscribe(ex => this.Log().Warn("Failed to load user from session", ex));

            LoginScreenCommand2.Execute().ObserveOn(RxApp.MainThreadScheduler).Subscribe(user =>
            {
                if (user != null && _userSettings != null && _userSettings.ApplicationFolder != null && new DirectoryInfo(_userSettings.ApplicationFolder).Exists)
                {
                    MessageBus.Current.SendMessage(new LoggedInUserEvent { User = user });
                    Router.NavigateAndReset.Execute(new OffersViewModel(
                        Locator.Current.GetService<IOfferService>(),
                        Locator.Current.GetService<IUserService>(),
                        Locator.Current.GetService<IDialogService>()));
                }
                else
                {
                    ShowLoginScreen();
                }
            });
        }

        private void SetApplicationFolder()
        {
            if (!string.IsNullOrEmpty(_userSettings?.ApplicationFolder))
            {
                var path = _userSettings.ApplicationFolder;
                var di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    PathFactory.SetApplicationFolder(_userSettings?.ApplicationFolder!);
                }
                else
                {
                    Log.Logger.Warning($"directory not exists : {_userSettings?.ApplicationFolder}");
                }
            }
        }

        private async Task<Unit> LogoutUser()
        {
            var sessionPath = PathFactory.GetSessionFile();
            var fileSessionInfo = new FileInfo(sessionPath);
            if (fileSessionInfo.Exists)
            {
                File.WriteAllText(fileSessionInfo.FullName, string.Empty);
            }
            await LoginScreenCommand.Execute();

            return Unit.Default;
        }

        [Reactive] public bool IsLoggedIn { get; set; }
        [Reactive] public string UserName { get; set; }
        [Reactive] public string UserRole { get; set; }

        public ReactiveCommand<Unit, Unit> LoginScreenCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, IRoutableViewModel> NavigateBackCommand { get; }
        public ReactiveCommand<Unit, Unit> ProfilCommand { get; }
        public IObservable<bool> CanNavigateBack { get; }
        public bool IsBackButtonVisible { get; private set; }

        public RoutingState Router { get; }
        public User? User { get; private set; }

        private void ShowLoginScreen()
        {
            _userSettings.User = null;
            IsLoggedIn = false;
            Router.NavigateAndReset.Execute(new LoginViewModel(Locator.Current.GetService<IFileChooser>()!, Locator.Current.GetService<IUserSettings>()!, this));
        }

        private async Task<User?> GetUserFromSession(Unit unit)
        {
            try
            {
                var sessionPath = PathFactory.GetSessionFile();
                var appConfigPath = PathFactory.GetAppConfigFile();//
                var sessionExist = await CheckSession(sessionPath);
                if (sessionExist)
                {
                    var appConfig = await _appConfigSource.Get(appConfigPath);
                    if (appConfig != null)
                    {
                        var session = await _sessionSource.Get(sessionPath);
                        var user = appConfig?.Users?.Where(user => user.Email?.Equals(session?.UserName) ?? false).FirstOrDefault();
                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message, e);
                return null;
            }

        }

        private async Task<bool> CheckSession(string sessionPath)
        {
            try
            {
                var fileInfo = new FileInfo(sessionPath);
                var userEmail = _userSettings.LastUserEmail;
                if (userEmail is not null && _sessionSource is not null && fileInfo.Exists)
                {
                    var session = await _sessionSource.Get(sessionPath);
                    if (session?.UserName?.Equals(userEmail) ?? false)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex?.Message, ex, ex?.StackTrace?.ToString());
                return false;
            }
        }

        private void ShowProfileScreen()
        {
            Router.Navigate.Execute(new ProfileViewModel(this));
        }
    }
}
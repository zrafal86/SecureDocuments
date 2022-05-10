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
    public sealed class LoginViewModel : ViewModelBase, IEnableLogger
    {
        private readonly IFileChooser _folderChooser;
        private readonly IUserSettings _settings;
        private readonly IAppConfigSource? _appConfigSource;
        private readonly ISessionSource? _sessionSource;

        public LoginViewModel(
            IFileChooser folderChooser,
            IUserSettings settings,
            IScreen? screen = null) : base(screen)
        {
            _folderChooser = folderChooser;
            _settings = settings;
            _appConfigSource = Locator.Current.GetService<IAppConfigSource>();
            _sessionSource = Locator.Current.GetService<ISessionSource>();
            ApplicationFolder = settings.ApplicationFolder;
            UserEmail = settings.LastUserEmail;

            var canLogin = this.WhenAnyValue(vm => vm.UserPassword, vm => vm.UserEmail, (pass, email) =>
                !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(pass) && pass?.Length >= 5);
            LoginCommand = ReactiveCommand.CreateFromTask<Unit, bool>(DoLogin, canLogin);
            LoginCommand
                .ThrownExceptions
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async exception =>
                {
                    this.Log().Error("Error!", exception);
                    await _dialogService.ShowDialog("", exception.Message, true);
                });
            ChooseAppDirCommand = ReactiveCommand.Create(SelectAppFolder);
        }

        public override string UrlPathSegment => "Login";
        public ReactiveCommand<Unit, bool> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> ChooseAppDirCommand { get; }

        [Reactive] public string? UserEmail { get; set; }
        [Reactive] public string? UserPassword { get; set; }
        [Reactive] public string? ApplicationFolder { get; set; }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --Login-- screen!");
            disposables.Add(this.WhenAnyValue(vm => vm.ApplicationFolder)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(folder => SetUsersSource(folder)));

            disposables.Add(LoginCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(success =>
            {
                if (success)
                {
                    HostScreen.Router.NavigateAndReset.Execute(new OffersViewModel(
                        Locator.Current.GetService<IOfferService>(),
                        Locator.Current.GetService<IUserService>(),
                        Locator.Current.GetService<IDialogService>()));
                }
            }));
        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --Login-- screen!");
        }

        private void SetUsersSource(string? folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                PathFactory.SetApplicationFolder(folder);
                var configFile = PathFactory.GetAppConfigFile();
                var file = new FileInfo(configFile);
                if (file.Exists && _appConfigSource is not null)
                {
                }
                else
                {
                    MessageBus.Current.SendMessage(new CannotFindConfigFileEvent { Message = $"Cannot find file: {configFile}" });
                }
            }
        }

        private void SelectAppFolder()
        {
            var appFolder = _folderChooser.ChooseFolder();
            PathFactory.SetApplicationFolder(appFolder);
            _settings.ApplicationFolder = appFolder;
            ApplicationFolder = appFolder;
        }

        private async Task<bool> DoLogin(Unit arg)
        {
            var config = await _appConfigSource!.Get(PathFactory.GetAppConfigFile());
            var (success, user) = Login(UserEmail!, UserPassword!, config?.Users);
            if (success && user != null)
            {
                _settings.LastUserEmail = user.Email ?? "";
                _settings.User = user;
                var sessionFile = PathFactory.GetSessionFile();
                var file = new FileInfo(sessionFile);
                if (file.Exists is false)
                {
                    file.Create();
                }
                MessageBus.Current.SendMessage(new LoggedInUserEvent { User = user });
            }
            else
            {
                MessageBus.Current.SendMessage(new InvalidUserLoginEvent());
                await _dialogService.ShowDialog("Cannot login", "Coś jest nie tak! \nZłe hasło? Zły login? \nA może nie ma pliku [app_config.dtms]?");
            }

            return success;
        }

        private (bool success, User? User) Login(string login, string password, IEnumerable<User>? users)
        {
            var user = users?.FirstOrDefault(u => u.Email!.ToLowerInvariant().Equals(login.Trim().ToLowerInvariant()));
            if (user is not null && password is not null)
            {
                var hashed = Hash.DoHash(password);
                if (hashed != null && hashed.Equals(user.PasswordHash))
                {
                    _sessionSource?.Save(new Session { UserName = user.Email }, PathFactory.GetSessionFile());
                    return (true, user);
                };
            }

            return (false, null);
        }
    }
}
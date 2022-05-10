using SecureDocuments.Data;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Factories;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using SecureDocuments.Services.Hash;
using SecureDocuments.ViewModels;
using SecureDocuments.WPF.Services;
using ReactiveUI;
using Splat;
using System.Reflection;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IEnableLogger
    {
        private IMutableDependencyResolver Kernel = Locator.CurrentMutable;

        public MainWindow()
        {
            Kernel.InitializeReactiveUI();
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            void register()
            {
                //Services
                Kernel.Register(() => new HashService(), typeof(IHashCalculator));
                Kernel.Register(() => new KeysFactory(), typeof(IKeysFactory));
                var keyFactory = Locator.Current.GetService<IKeysFactory>();
                if (keyFactory != null)
                {
                    Kernel.Register(() => new KeyProvider(keyFactory), typeof(IKeyProvider));
                }
                Kernel.RegisterConstant(new AesEncryption(
                    Locator.Current.GetService<IKeyProvider>()),
                    typeof(ISymmetricEncryption));
                Kernel.Register(() => new FileCategoryNamesSource(), typeof(ICategoryNamesSource));
                Kernel.RegisterConstant(new UserSettings(), typeof(IUserSettings));
                Kernel.RegisterConstant(new ThemeService(), typeof(IThemeService));
                Kernel.RegisterConstant(new ResourceService(), typeof(IResourceService));

                Kernel.Register(() => new AppConfigSource(
                    Locator.Current.GetService<ISymmetricEncryption>()!), typeof(IAppConfigSource));
                Kernel.Register(() => new SessionSource(
                    Locator.Current.GetService<ISymmetricEncryption>()!), typeof(ISessionSource));


                Kernel.RegisterConstant(new UserService(
                    Locator.Current.GetService<IUserSettings>()!), typeof(IUserService));
                Kernel.Register(() => new EmailConfigNotification(
                    Locator.Current.GetService<IUserService>()!), typeof(SecureDocuments.Services.Notification));
                Kernel.Register(() => new RoleAccessService(), typeof(IRoleAccessService));
                Kernel.Register(() => new FileChooser(), typeof(IFileChooser));
                Kernel.Register(() => new DialogService(), typeof(IDialogService));
                Kernel.Register(() => new OfferSource(
                    Locator.Current.GetService<ISymmetricEncryption>()!), typeof(IOfferSource));
                Kernel.Register(() => new FileOfferSource(
                    Locator.Current.GetService<ISymmetricEncryption>()!), typeof(IFileOfferSource));
                Kernel.Register(() => new OffersService(
                    Locator.Current.GetService<IUserSettings>()!,
                    Locator.Current.GetService<ISymmetricEncryption>()!,
                    Locator.Current.GetService<IOfferSource>()!), typeof(IOfferService));
                Kernel.Register(() => new FilesService(), typeof(IFilesService));

                //Views - ViewModels
                Kernel.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            }

            ViewModel = new MainViewModel(Locator.CurrentMutable, register);

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.IsLoggedIn,
                        view => view.LogoutPanel.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.UserName,
                        view => view.UserNameTextBlock.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.UserRole,
                        view => view.RoleTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.LogoutCommand,
                        view => view.LogoutButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.NavigateBackCommand,
                        view => view.BackButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.ProfilCommand,
                        view => view.ProfilButton)
                    .DisposeWith(disposables);
            });
            SetupTheme(Locator.Current.GetService<IUserSettings>()!, Locator.Current.GetService<IThemeService>()!);
        }

        private static void SetupTheme(IUserSettings userSettings, IThemeService themeService)
        {
            var isDark = userSettings.IsDarkTheme;
            themeService.SetTheme(isDark);
        }

        protected override void OnActivated(System.EventArgs e)
        {
            base.OnActivated(e);
            _ = MessageBus.Current.Listen<CannotFindConfigFileEvent>()
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async eventObject =>
                {
                    var dialogService = Locator.Current.GetService<IDialogService>();
                    _ = await dialogService!.ShowDialog("Problem", eventObject.Message ?? "error", true);
                });
        }

        protected override void OnDeactivated(System.EventArgs e)
        {
            base.OnDeactivated(e);
        }
    }
}
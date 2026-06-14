using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.Data;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Factories;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using SecureDocuments.Services.Hash;
using SecureDocuments.ViewModels;
using SecureDocuments.Avalonia.Services;
using Splat;
using System.Reflection;

namespace SecureDocuments.Avalonia.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    private IMutableDependencyResolver Kernel = Locator.CurrentMutable;

    public MainWindow()
    {
        Kernel.InitializeReactiveUI();
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowState = WindowState.Maximized;

        FileChooser.SetMainWindow(this);

        void register()
        {
            Kernel.Register(() => new HashService(), typeof(IHashCalculator));
            Kernel.Register(() => new KeysFactory(), typeof(IKeysFactory));
            var keyFactory = Locator.Current.GetService<IKeysFactory>();
            if (keyFactory != null)
                Kernel.Register(() => new KeyProvider(keyFactory), typeof(IKeyProvider));

            Kernel.RegisterConstant(new AesEncryption(
                Locator.Current.GetService<IKeyProvider>()), typeof(ISymmetricEncryption));
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

            Kernel.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }

        ViewModel = new MainViewModel(Locator.CurrentMutable, register);

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    vm => vm.IsLoggedIn,
                    view => view.LogoutPanel.IsVisible)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    vm => vm.UserName,
                    view => view.UserNameRun.Text)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel,
                    vm => vm.UserRole,
                    view => view.RoleRun.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.LogoutCommand, view => view.LogoutButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.NavigateBackCommand, view => view.BackButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ProfilCommand, view => view.ProfilButton)
                .DisposeWith(disposables);
        });

        SetupTheme(Locator.Current.GetService<IUserSettings>()!, Locator.Current.GetService<IThemeService>()!);

        _ = MessageBus.Current.Listen<CannotFindConfigFileEvent>()
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async evt =>
            {
                var dialogService = Locator.Current.GetService<IDialogService>();
                _ = await dialogService!.ShowDialog("Problem", evt.Message ?? "error", true);
            });
    }

    private static void SetupTheme(IUserSettings userSettings, IThemeService themeService)
    {
        themeService.SetTheme(userSettings.IsDarkTheme);
    }
}

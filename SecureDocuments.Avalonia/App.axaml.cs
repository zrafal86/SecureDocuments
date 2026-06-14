using Avalonia;
using Avalonia.Markup.Xaml;
using Serilog;
using Splat;
using Splat.Serilog;

namespace SecureDocuments.Avalonia;

public partial class App : Application
{
    public App()
    {
        var culture = CultureInfo.CreateSpecificCulture("pl-PL");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dmfs-.log");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(fileName, rollingInterval: RollingInterval.Day)
#if DEBUG
            .MinimumLevel.Debug()
            .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
#else
            .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
#endif
            .CreateLogger();

        Locator.CurrentMutable.UseSerilogFullLogger();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new Views.MainWindow();
            desktop.MainWindow = mainWindow;
        }

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Logger.Error((Exception)e.ExceptionObject, "Unhandled exception");
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Logger.Error(e.Exception, "Unobserved task exception");
        };

        base.OnFrameworkInitializationCompleted();
    }
}

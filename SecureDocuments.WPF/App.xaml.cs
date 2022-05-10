using SecureDocuments.WPF.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Serilog;
using Splat;
using Splat.Serilog;

namespace SecureDocuments.WPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //"en"; "pl-PL"
            var culture = CultureInfo.CreateSpecificCulture("pl-PL");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        //private readonly DateTimeOffset expireDate = DateTimeOffset.ParseExact("12/18/2021 14:14:00 +02:00", "MM/dd/yyyy H:mm:ss zzz", CultureInfo.InvariantCulture);

        protected override void OnStartup(StartupEventArgs e)
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

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            var view = new MainWindow();
            view.Show();

            base.OnStartup(e);
            AppCenter.Start("ea40fd41-82f6-47c5-958d-eddb27c3be32",
                   typeof(Analytics), typeof(Crashes));

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            //var now = DateTimeOffset.UtcNow;
            //var result = DateTimeOffset.Compare(expireDate, now);

            //if (result < 0) throw new Exception("Something went wrong. Please contact with Developer");
        }

        private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Crashes.TrackError(e?.Exception);
            ReportCrash(e?.Exception);
            Environment.Exit(0);
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Crashes.TrackError(e?.Exception);
            ReportCrash(e?.Exception);
            Environment.Exit(0);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crashes.TrackError((Exception)e.ExceptionObject);
            ReportCrash((Exception)e.ExceptionObject);
            Environment.Exit(0);
        }

        public static void ReportCrash(Exception? exception,
            string developerMessage = "Something went wrong. Please contact with Developer")
        {
            Log.Logger.Error(exception?.Message ?? developerMessage, exception, exception?.StackTrace?.ToString());
        }
    }
}
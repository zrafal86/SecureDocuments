using SecureDocuments.ConsoleUtils.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace SecureDocuments.ConsoleUtils
{
    internal class Program
    {
        protected Program() { }

        private static void Main(string[] args)
        {
            Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<AppConfig>(hostContext.Configuration.GetSection("AppConfig"));
                })
#if DEBUG
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddZLoggerRollingFile((dt, x) => $"logs/{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
                        x => x.ToLocalTime().Date, 1024);
                    logging.AddZLoggerConsole();
                })
#endif
                .RunConsoleAppFrameworkAsync(args, new ConsoleAppOptions
                {
                    GlobalFilters = new ConsoleAppFilter[]
                    {
                        new MutexFilter {Order = -9999},
                        new LogRunningTimeFilter {Order = -9998}
                    }
                });
        }
    }
}
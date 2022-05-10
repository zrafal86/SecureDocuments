using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SecureDocuments.ConsoleUtils.Filters
{
    public class LogRunningTimeFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            context.Logger.LogInformation($"Call method at {context.Timestamp.ToLocalTime()}");
            try
            {
                await next(context);
                context.Logger.LogInformation($"Call method Completed successfully, Elapsed: {DateTimeOffset.UtcNow - context.Timestamp}");
            }
            catch
            {
                context.Logger.LogInformation($"Call method Completed Failed, Elapsed: {DateTimeOffset.UtcNow - context.Timestamp}");
                throw;
            }
        }
    }
}
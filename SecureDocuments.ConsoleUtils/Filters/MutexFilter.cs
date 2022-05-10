using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureDocuments.ConsoleUtils.Filters
{
    public class MutexFilter : ConsoleAppFilter
    {
        public override async ValueTask Invoke(ConsoleAppContext context, Func<ConsoleAppContext, ValueTask> next)
        {
            var name = context.MethodInfo.DeclaringType?.Name + "." + context.MethodInfo.Name;
            using var mutex = new Mutex(true, name, out var createdNew);
            if (!createdNew) throw new Exception($"already running {name} in another process.");

            await next(context);
        }
    }
}
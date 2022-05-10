using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace SecureDocuments.ConsoleUtils
{
    public class Offers : ConsoleAppBase
    {
        private readonly IOptions<AppConfig> config;

        // get configuration from DI.
        public Offers(IOptions<AppConfig> config)
        {
            this.config = config;
        }

        public void Action(
            [Option("o", "method name to invoke.")]
            string name,
            [Option("p", "param to method")] string param = "")
        {
            Context.Logger.LogDebug($"Hello My ConsoleApp invoke method '{name}' with param '{param}'");
            switch (name)
            {
                case "AddOffer":
                    AddOffer();
                    break;

                default:
                    throw new ArgumentException(
                        "method name is not a recognized",
                        nameof(name));
            }

            Context.Logger.LogDebug($"GlobalValue: {config.Value.GlobalValue}, EnvValue: {config.Value.EnvValue}");
        }

        private void AddOffer()
        {
            Context.Logger.LogDebug("AddOffer method");
        }
    }
}
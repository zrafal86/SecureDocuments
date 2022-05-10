using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SecureDocuments.Data
{
    public static class JsonSerializerConfig
    {
        public static JsonSerializerSettings GetConfig()
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = false
                }
            };
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = contractResolver,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            return settings;
        }
    }
}
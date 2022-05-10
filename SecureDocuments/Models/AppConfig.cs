using Newtonsoft.Json;

namespace SecureDocuments.Models
{
    public record AppConfig
    {
        [JsonProperty("Users")] public IEnumerable<User>? Users { get; init; }
        [JsonProperty("EmailNotification")] public EmailNotification? EmailNotification { get; init; }
        [JsonProperty("CompanyNames")] public IEnumerable<CompanyName>? CompanyNames { get; init; }
        [JsonProperty("Subjects")] public IEnumerable<Subject>? Subjects { get; init; }
        [JsonProperty("ListOfPeopleToBeInformedWhenOfferAccepted")] public IEnumerable<User>? ListOfPeopleToBeInformedWhenOfferAccepted { get; init; }
        [JsonProperty("CustomerCountries")] public IEnumerable<CustomerCountry>? CustomerCountries { get; init; }
    }

    public record EmailNotification
    {
        public int Port { get; init; }
        public string Host { get; init; } = "";
        public string CredentialUserName { get; init; } = "";
        public string CredentialPassword { get; init; } = "";
        public bool EnableSsl { get; init; }
        public int Timeout { get; init; }
        public string From { get; init; } = "";
    }

    public record CompanyName
    {
        public string Name { get; init; } = "";
    }

    public record Subject
    {
        public string Name { get; init; } = "";
    }

    public record CustomerCountry
    {
        public int Id { get; init; }
        public string ShortName { get; init; } = "";
        public string OfficialName { get; init; } = "";
        public string ISO2 { get; init; } = "";
    }
}

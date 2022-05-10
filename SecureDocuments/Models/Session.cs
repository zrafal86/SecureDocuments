using Newtonsoft.Json;

namespace SecureDocuments.Models
{
    public record Session
    {
        [JsonProperty("user_name")] public string? UserName { get; init; }
    }
}

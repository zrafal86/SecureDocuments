using Newtonsoft.Json;

namespace SecureDocuments.Models
{
    public record User
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }

        [JsonIgnore] public string FullName => $"{FirstName} {LastName}";

        public string? Email { get; init; }

        public string? Phone { get; init; }

        [JsonProperty("password_hash")] public string? PasswordHash { get; init; }

        public Role Role { get; init; }
    }
}
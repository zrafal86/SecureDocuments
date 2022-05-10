using SecureDocuments.Models;

namespace SecureDocuments.Encryption.Symmetric
{
    public record RoleKey : IKey
    {
        public RoleKey(string key, string iv, Role role)
        {
            Role = role;
            Key = key;
            IV = iv;
        }

        public Role Role { get; init; }
        public string IV { get; init; }
        public string Key { get; init; }
    }
}
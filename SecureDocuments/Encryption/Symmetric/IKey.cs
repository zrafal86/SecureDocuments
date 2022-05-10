using SecureDocuments.Models;

namespace SecureDocuments.Encryption.Symmetric
{
    public interface IKey
    {
        string Key { get; }
        string IV { get; }
        Role Role { get; }
    }
}
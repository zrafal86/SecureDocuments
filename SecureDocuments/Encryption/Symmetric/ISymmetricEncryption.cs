using SecureDocuments.Models;

namespace SecureDocuments.Encryption.Symmetric
{
    public interface ISymmetricEncryption
    {
        Task<string> Encrypt(string data, Role role);

        Task<string> Decrypt(string data, Role role);
    }
}
using SecureDocuments.Models;
using System.Security.Cryptography;

namespace SecureDocuments.Encryption.Symmetric
{
    public interface IKeyProvider
    {
        IKey ProvideKey(Role role);
        SymmetricAlgorithm ProvideCryptoService(IKey keyRole);
        SymmetricAlgorithm ProvideCryptoService(byte[] key, byte[] iv);
    }
}
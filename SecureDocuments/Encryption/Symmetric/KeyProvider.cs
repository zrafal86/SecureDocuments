using SecureDocuments.Models;
using System.Security.Cryptography;

namespace SecureDocuments.Encryption.Symmetric
{
    public record KeyProvider : IKeyProvider
    {
        private IEnumerable<IKey> AllKeys { get; init; }

        public KeyProvider(IKeysFactory faktory)
        {
            AllKeys = faktory.CreateKeys();
        }

        public IKey ProvideKey(Role role)
        {
            return AllKeys.Where(key => key.Role == role).First();
        }

        public SymmetricAlgorithm ProvideCryptoService(IKey keyRole)
        {
            return ProvideCryptoService(
                Convert.FromBase64String(keyRole.Key),
                Convert.FromBase64String(keyRole.IV));
        }

        public SymmetricAlgorithm ProvideCryptoService(byte[] key, byte[] iv)
        {
            return new AesCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,

                Key = key,
                IV = iv
            };
        }
    }
}
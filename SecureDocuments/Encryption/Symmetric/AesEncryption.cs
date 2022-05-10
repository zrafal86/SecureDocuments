using SecureDocuments.Models;
using Splat;
using System.Security.Cryptography;

namespace SecureDocuments.Encryption.Symmetric
{
    public sealed class AesEncryption : ISymmetricEncryption
    {
        public IKeyProvider KeyProvider { get; init; }
        public AesEncryption(IKeyProvider? keyProvider = null)
        {
            KeyProvider = keyProvider ?? Locator.Current.GetService<IKeyProvider>();
        }

        public async Task<string> Encrypt(string data, Role role)
        {
            var key = KeyProvider.ProvideKey(role);
            return await Encrypt(data, key.Key, key.IV);
        }

        public async Task<string> Decrypt(string data, Role role)
        {
            var key = KeyProvider.ProvideKey(role);
            return await Decrypt(data, key.Key, key.IV);
        }

        private async Task<string> Encrypt(string dataToEncrypt, string key, string iv)
        {
            var encrypted = await Task.Run(() =>
            {
                return Encrypt(
                    Encoding.UTF8.GetBytes(dataToEncrypt),
                    Convert.FromBase64String(key),
                    Convert.FromBase64String(iv));
            });
            return Convert.ToBase64String(encrypted);
        }

        private async Task<string> Decrypt(string dataToDecrypt, string key, string iv)
        {
            var decrypted = await Task.Run(() =>
            {
                return Decrypt(
                    Convert.FromBase64String(dataToDecrypt),
                    Convert.FromBase64String(key),
                    Convert.FromBase64String(iv));
            });
            return Encoding.UTF8.GetString(decrypted);
        }

        private byte[] Encrypt(byte[] dataToEncrypt, byte[] key, byte[] iv)
        {
            using var aes = KeyProvider.ProvideCryptoService(key, iv);

            using var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }

        private byte[] Decrypt(byte[] dataToDecrypt, byte[] key, byte[] iv)
        {
            using var aes = KeyProvider.ProvideCryptoService(key, iv);

            using var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
            cryptoStream.FlushFinalBlock();

            var decryptBytes = memoryStream.ToArray();

            return decryptBytes;
        }
    }
}
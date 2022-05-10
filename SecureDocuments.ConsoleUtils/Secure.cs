using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecureDocuments.ConsoleUtils
{
    public sealed class Secure : ConsoleAppBase
    {
        private readonly IOptions<AppConfig> config;

        public ISymmetricEncryption Encryption { get; }

        public Secure(IOptions<AppConfig> config, ISymmetricEncryption? encryption = null)
        {
            this.config = config;
            ISymmetricEncryption symmetricEncryption = encryption ?? CreateEncryptor();
            Encryption = symmetricEncryption;
        }

        private static ISymmetricEncryption CreateEncryptor()
        {
            IKeysFactory keysFactory = new KeysFactory();
            IKeyProvider keyProvider = new KeyProvider(keysFactory);
            return new AesEncryption(keyProvider);
        }

        public void Run(
            [Option("method", "method name to invoke.")] string method,
            [Option("in", "input file")] string input,
            [Option("out", "output file")] string output = "",
            [Option("role", "role key encrypted.")] int role = (int)Role.Creator)
        {
            Context.Logger.LogDebug($"Hello My ConsoleApp invoke method '{method}' with param '{input}'");
            switch (method)
            {
                case "encrypt":
                    if (input != null)
                    {
                        if (input.EndsWith(".details") || input.EndsWith(".dtms") || input.EndsWith(".json"))
                        {
                            var newContent = Encrypt(input, role);
                            File.WriteAllText(output, newContent, Encoding.UTF8);
                        }
                        else
                        {
                            EncryptFile(input, output, role);
                        }
                    }
                    break;
                case "decrypt":
                    if (input != null)
                    {
                        if (input.EndsWith(".details") || input.EndsWith(".dtms"))
                        {
                            var decryptedContent = Decrypt(input, role);
                            File.WriteAllText(output, decryptedContent, Encoding.UTF8);
                        }
                        else
                        {
                            DecryptFile(input, output, role);
                        }
                    }
                    break;

                default:
                    throw new ArgumentException(
                        "method name is not a recognized",
                        nameof(method));
            }

            Context.Logger.LogDebug($"GlobalValue: {config.Value.GlobalValue}, EnvValue: {config.Value.EnvValue}");
        }

        private static void EncryptFile(string input, string output, int role)
        {
            var _keyProvider = new KeyProvider(new KeysFactory());
            var keyRole = _keyProvider.ProvideKey((Role)role);

            using var sourceStream = File.OpenRead(input);
            using var destinationStream = File.Create(output);
            using var provider = _keyProvider.ProvideCryptoService(keyRole);
            using var cryptoTransform = provider.CreateEncryptor();
            using var cryptoStream = new CryptoStream(destinationStream, cryptoTransform, CryptoStreamMode.Write);
            destinationStream.Write(provider.IV, 0, provider.IV.Length);
            sourceStream.CopyTo(cryptoStream);
        }

        private static void DecryptFile(string input, string output, int role)
        {
            var _keyProvider = new KeyProvider(new KeysFactory());
            var keyRole = _keyProvider.ProvideKey((Role)role);
            var key = Convert.FromBase64String(keyRole.Key);

            using var sourceStream = File.OpenRead(input);
            using var destinationStream = File.Create(output);
            using var provider = _keyProvider.ProvideCryptoService(keyRole);
            var IV = new byte[provider.IV.Length];
            sourceStream.Read(IV, 0, IV.Length);

            using var cryptoTransform = provider.CreateDecryptor(key, IV);
            using var cryptoStream = new CryptoStream(sourceStream, cryptoTransform, CryptoStreamMode.Read);
            cryptoStream.CopyTo(destinationStream);
        }

        private string Encrypt(string input, int role)
        {
            var fileContent = File.ReadAllText(input, Encoding.UTF8);
            var newContent = Encryption.Encrypt(fileContent, (Role)role).Result;
            return newContent;
        }

        private string Decrypt(string input, int role)
        {
            var content = File.ReadAllText(input, Encoding.UTF8);
            var decryptedContent = Encryption.Decrypt(content, (Role)role).Result;
            return decryptedContent;
        }
    }
}
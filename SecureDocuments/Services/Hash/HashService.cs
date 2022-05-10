using System.Security.Cryptography;

namespace SecureDocuments.Services.Hash
{
    public interface IHashCalculator
    {
        string CalculateHash(string filePath, HashAlgorithm? algorithm = null);
    }

    public class HashService : IHashCalculator
    {
        public string CalculateHash(string filePath, HashAlgorithm? algorithm = null)
        {
            algorithm ??= SHA512.Create();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var bytes = algorithm.ComputeHash(stream);

            var builder = new StringBuilder();
            foreach (var t in bytes) builder.Append(t.ToString("x2"));

            return builder.ToString();
        }
    }
}
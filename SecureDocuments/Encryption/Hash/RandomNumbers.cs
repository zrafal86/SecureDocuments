using System.Security.Cryptography;

namespace SecureDocuments.Encryption.Hash
{
    //GenerateRandomNumber(32) for key
    //GenerateRandomNumber(16) for iv
    public class RandomNumbers
    {
        public static string Generate()
        {
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            var randomNumber = new byte[32];
            randomNumberGenerator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}
using System.Security.Cryptography;

namespace SecureDocuments.Encryption.Hash
{
    public class Hash
    {
        private const int NUMBER_OF_ROUNDS = 20000;
        private const string SALT = "M2gFMQWHEEZ7O1TRhOZmjB0e7w5Cltcn0rwiCunFGJg=";

        public static string DoHash(string toBeHashed)
        {
            using var rfc2898 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(toBeHashed),
                Convert.FromBase64String(SALT), NUMBER_OF_ROUNDS);
            return Convert.ToBase64String(rfc2898.GetBytes(32));
        }
    }
}
namespace SecureDocuments.Encryption.Symmetric
{
    public interface IKeysFactory
    {
        IEnumerable<IKey> CreateKeys();
    }
}
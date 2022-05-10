using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;

namespace SecureDocuments.Data
{
    public interface ISessionSource
    {
        Task<Session?> Get(string filePath);

        Task Save(Session session, string filePath);
    }

    public class SessionSource : DataSourceBase<Session>, ISessionSource
    {
        public SessionSource(ISymmetricEncryption encryption) : base(encryption)
        {
        }

        public override Task<Session?> Get(string filePath) => base.Get(filePath);

        public override Task Save(Session config, string filePath) => base.Save(config, filePath);
    }
}

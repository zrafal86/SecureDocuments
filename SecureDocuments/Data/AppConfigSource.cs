using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;

namespace SecureDocuments.Data
{
    public interface IAppConfigSource
    {
        Task<AppConfig?> Get(string filePath);

        Task Save(AppConfig config, string filePath);
    }

    public class AppConfigSource : DataSourceBase<AppConfig>, IAppConfigSource
    {
        public AppConfigSource(ISymmetricEncryption encryption) : base(encryption)
        {
        }

        public override Task<AppConfig?> Get(string filePath) => base.Get(filePath);

        public override Task Save(AppConfig config, string filePath) => base.Save(config, filePath);
    }
}
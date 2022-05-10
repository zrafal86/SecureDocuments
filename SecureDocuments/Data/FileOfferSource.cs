using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;

namespace SecureDocuments.Data
{
    public interface IFileOfferSource
    {
        Task<FileDetails?> Get(string filePath);
        Task Save(FileDetails item, string filePath);
    }

    public sealed class FileOfferSource : DataSourceBase<FileDetails>, IFileOfferSource
    {
        public FileOfferSource(ISymmetricEncryption encryption) : base(encryption)
        {
        }

        public override Task<FileDetails?> Get(string filePath)
        {
            return base.Get(filePath);
        }

        public override Task Save(FileDetails item, string filePath)
        {
            return base.Save(item, filePath);
        }
    }
}
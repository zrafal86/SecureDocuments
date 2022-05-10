using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Models;

namespace SecureDocuments.Data
{
    public interface IOfferSource
    {
        Task<Offer?> Get(string filePath);

        Task Save(Offer offer, string filePath);
    }

    public sealed class OfferSource : DataSourceBase<Offer>, IOfferSource
    {
        public OfferSource(ISymmetricEncryption encryption) : base(encryption)
        {
        }

        public override Task<Offer?> Get(string filePath)
        {
            return base.Get(filePath);
        }

        public override Task Save(Offer item, string filePath)
        {
            return base.Save(item, filePath);
        }
    }
}
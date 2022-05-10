using SecureDocuments.Data;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Extensions;
using SecureDocuments.Models;
using SecureDocuments.Models.Events;
using ReactiveUI;
using Splat;

namespace SecureDocuments.Services
{
    public interface IOfferService
    {
        IAsyncEnumerable<Offer> GetOffers();
        Task<bool> SetOffer(Offer offer);
    }

    public sealed class OffersService : IOfferService, IEnableLogger
    {
        private readonly IUserSettings _userSettings;
        private readonly ISymmetricEncryption _encryption;
        private readonly IOfferSource _offerSource;

        public OffersService(
            IUserSettings userSettings,
            ISymmetricEncryption encryption,
            IOfferSource offerSource) : base()
        {
            _userSettings = userSettings;
            _encryption = encryption;
            _offerSource = offerSource;
        }

        public async IAsyncEnumerable<Offer> GetOffers()
        {
            var files = await GetApplicationFiles();

            var container = files.Where(fi => fi.Name.Equals(ConstantValues.OfferFile)).ToList();

            foreach (var offerFile in container)
            {
                var offer = await _offerSource.Get(offerFile.FullName);
                if (offer != null)
                {
                    offer.CreatedTime = offer.CreatedTime.ToLocalTime();
                    yield return offer;
                }
            }
        }

        public async Task<bool> SetOffer(Offer offer)
        {
            try
            {
                var isSuccess = false;
                var files = await GetApplicationFiles();

                var container = files.Where(fi => fi.Name.Equals(ConstantValues.OfferFile)).ToList();
                foreach (var offerFile in container)
                {
                    var currentOffer = await _offerSource.Get(offerFile.FullName);
                    if (currentOffer.Id.Equals(offer.Id))
                    {
                        await _offerSource.Save(offer, offerFile.FullName);
                        isSuccess = true;
                    }
                }

                return isSuccess;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<List<FileInfo>> GetApplicationFiles()
        {
            return await Task.Run(async () =>
            {
                var listOfFiles = new List<FileInfo>();
                if (_userSettings.ApplicationFolder != null)
                {
                    var di = new DirectoryInfo(_userSettings.ApplicationFolder);
                    var filePaths = await di.WalkDirectoryTreeAsync(ConstantValues.ApplicationFileExtension, true);
                    if (filePaths != null)
                    {
                        var fileInfos = filePaths.Select(path => new FileInfo(path));
                        listOfFiles.AddRange(fileInfos);
                    }
                }
                else
                {
                    MessageBus.Current.SendMessage(new ApplicationFolderIsNotSetEvent());
                }
                return listOfFiles;
            });
        }
    }
}
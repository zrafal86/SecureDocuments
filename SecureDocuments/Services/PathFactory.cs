#nullable enable

using SecureDocuments.Models;

namespace SecureDocuments.Services
{
    public static class PathFactory
    {
        public static string ApplicationFolder { get; private set; }

        public static string GetPathToNewOfferFile(string applicationFolder, Offer offer)
        {
            var offerFolder = Path.Combine(applicationFolder, offer.Name?.Replace(" ", "-") + "_" + offer.Id);
            var offerFilePath = Path.Combine(offerFolder, ConstantValues.OfferFile);
            CreateDirectoryIDoNotExist(offerFolder);
            return offerFilePath;
        }

        public static string GetOfferDirectory(Offer offer) =>
            Path.Combine(ApplicationFolder, offer.Name?.Replace(" ", "-") + "_" + offer.Id);

        internal static string GetUploadDestinationFile(FileInfo fi, string uploadFolder) =>
            Path.Combine(uploadFolder, fi.Name + ConstantValues.OfferFileExtension);

        internal static string GetDownloadDestinationFile(FileInfo fi, string downloadFolder)
        {
            var parts = fi.Name.Split('.');
            string destinationFile;
            if (parts.Length > 2)
            {
                var last = parts.Length - 1;
                var builder = new StringBuilder();
                for (var i = 0; i < parts.Length; i++)
                {
                    if (i == last) break;
                    if (i != 0) builder.Append('.');
                    builder.Append(parts[i]);
                }

                destinationFile = Path.Combine(downloadFolder, builder.ToString());
            }
            else
            {
                destinationFile = Path.Combine(downloadFolder, parts[0] + "." + parts[1]);
            }

            return destinationFile;
        }

        public static string? GetOfferDirectoryPath(Offer offer) //TODO: offer folder located here
        {
            var di = new DirectoryInfo(ApplicationFolder);
            foreach (var dir in di.EnumerateDirectories())
            {
                if (offer != null && offer.Id != null && dir.Name.Contains(offer.Id))
                {
                    return dir.FullName;
                }
            }
            return null;
        }

        public static string? GetOfferInvoicesDirectoryPath(Offer offer) //TODO: invoice folder located here
        {
            var folder = GetOfferDirectoryPath(offer);
            if (folder != null)
            {
                var invoicesDirPath = Path.Combine(folder, ConstantValues.OfferInvoicesFolderName);
                CreateDirectoryIDoNotExist(invoicesDirPath);
                return invoicesDirPath;
            }
            return null;
        }

        private static void CreateDirectoryIDoNotExist(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        internal static string GetApplicationLocalFolderPath()
        {
            var localFilesDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                ConstantValues.ApplicationLocalFolderName);
            if (!Directory.Exists(localFilesDir))
            {
                Directory.CreateDirectory(localFilesDir);
            }
            return localFilesDir;
        }

        internal static string GetExportedOffersFile()
        {
            return Path.Combine(GetApplicationLocalFolderPath(), ConstantValues.ExportedOffersFile);
        }

        internal static string GetSessionFile()
        {
            var localSessionDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ConstantValues.ApplicationLocalFolderName);
            if (!Directory.Exists(localSessionDir))
            {
                Directory.CreateDirectory(localSessionDir);
            }
            return Path.Combine(localSessionDir, ConstantValues.SessionFileName);
        }

        /**
         * Gets path to file with the user data.
         */
        internal static string GetAppConfigFile() => Path.Combine(ApplicationFolder, ConstantValues.ConfigFile);

        internal static void SetApplicationFolder(string appFolder)
        {
            ApplicationFolder = appFolder;
        }

        internal static string GetDownloadDestinationFile(FolderPurpose purpose, Offer offer)
        {
            var subFolder = purpose switch
            {
                FolderPurpose.Normal => "",
                FolderPurpose.Invoices => ConstantValues.OfferInvoicesFolderName,
                _ => throw new NotImplementedException(),
            };
            var path = Path.Combine(GetApplicationLocalFolderPath(), offer.Id, subFolder);
            CreateDirectoryIDoNotExist(path);
            return path;
        }
    }
}
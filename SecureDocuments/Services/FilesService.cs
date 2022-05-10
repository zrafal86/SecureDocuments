using SecureDocuments.Data;
using SecureDocuments.Encryption.Symmetric;
using SecureDocuments.Extensions;
using SecureDocuments.Models;
using SecureDocuments.Models.Common;
using SecureDocuments.Models.File;
using SecureDocuments.Services.Hash;
using Serilog;
using Splat;
using System.Security.Cryptography;

namespace SecureDocuments.Services
{
    public interface IFilesService
    {
        Task<IEnumerable<FileDetails>> GetOfferFiles(string offerDir);
        Task<Result<FileDetails>> UploadFile(UploadFileParamDto uploadFileParams);
        Task<Result<string>> DownloadFile(DownloadFileParamDto downloadFileParams);
        Task<Result<FileDetails[]>> UploadFiles(UploadFilesParamDto uploadFilesParam);
        Task<Result<string[]>> DownloadFiles(DownloadFilesParamDto downloadFilesParam);
        Task<FileDetails?> GetFileDetail(string filePath);
    }

    public record UploadFileParamDto(string FilePath, UploadFileDetails UploadFileDetails, string ApplicationFolder, string OfferFolder);
    public record DownloadFileParamDto(FileDetails DetailsFile, string DestFolder, string ApplicationFolder, string OfferFolder);
    public record UploadFilesParamDto(string[] FilePaths, UploadFileDetails UploadFileDetails, string ApplicationFolder, string OfferFolder);
    public record DownloadFilesParamDto(FileDetails[] Details, string DestFolder, string ApplicationFolder, string OfferFolder);

    public sealed class FilesService : IFilesService
    {
        private readonly IFileOfferSource? _fileOfferSource;
        private readonly IHashCalculator? _hashService;
        private readonly IKeyProvider? _keyProvider;

        public FilesService(HashService? hashService = null, IKeyProvider? keyProvider = null,
            IFileOfferSource? fileOfferSource = null)
        {
            _hashService = hashService ?? Locator.Current.GetService<IHashCalculator>();
            _keyProvider = keyProvider ?? Locator.Current.GetService<IKeyProvider>();
            _fileOfferSource = fileOfferSource ?? Locator.Current.GetService<IFileOfferSource>();
        }

        public async Task<IEnumerable<FileDetails>> GetOfferFiles(string offerDir)
        {
            try
            {
                var files = await GetFiles(offerDir);
                var allOfferFiles = new List<FileDetails>();
                var container = files?.Select(filePath => new FileInfo(filePath)).Where(fi => fi.Extension.Equals(ConstantValues.OfferFileDetailsExtension));
                if (container != null && _fileOfferSource != null)
                {
                    foreach (var offerFile in container)
                    {
                        var currentOfferFile = await _fileOfferSource.Get(offerFile.FullName);
                        if (currentOfferFile != null)
                        {
                            allOfferFiles.Add(currentOfferFile!);
                        }
                    }
                }

                return allOfferFiles;
            }
            catch (Exception)
            {
                return Enumerable.Empty<FileDetails>();
            }
        }

        public async Task<Result<string>> DownloadFile(DownloadFileParamDto downloadFileParam)
        {
            Log.Logger.Debug($"\ndetails file info:\n{downloadFileParam.DetailsFile}");
            try
            {
                var file = new FileInfo(@$"{downloadFileParam.OfferFolder}\{downloadFileParam.DetailsFile.NameExt}{ConstantValues.OfferFileExtension}");

                var keyRole = _keyProvider?.ProvideKey(downloadFileParam.DetailsFile.Role);
                var destinationFileName = PathFactory.GetDownloadDestinationFile(file, downloadFileParam.DestFolder);

                var key = Convert.FromBase64String(keyRole!.Key);

                using var sourceStream = File.OpenRead(file.FullName);
                using var destinationStream = File.Create(destinationFileName);
                using var provider = _keyProvider?.ProvideCryptoService(keyRole!);
                var IV = new byte[provider!.IV.Length];
                sourceStream.Read(IV, 0, IV.Length);
                using var cryptoTransform = provider.CreateDecryptor(key, IV);
                using var cryptoStream = new CryptoStream(sourceStream, cryptoTransform, CryptoStreamMode.Read);
                cryptoStream.CopyTo(destinationStream);
            }
            catch (Exception e)
            {
                return await Task.FromResult(new Result<string>($"{downloadFileParam.DetailsFile.Name}{downloadFileParam.DetailsFile.RealExtension}",
                    new[] { new Error($"error: {e.Message}") }));
            }

            return await Task.FromResult(new Result<string>($"{downloadFileParam.DetailsFile.Name}{downloadFileParam.DetailsFile.RealExtension}"));
        }

        public async Task<Result<string[]>> DownloadFiles(DownloadFilesParamDto downloadFilesParam)
        {
            var listResult = new List<string>();
            if (downloadFilesParam.Details != null)
            {
                foreach (var detail in downloadFilesParam.Details)
                {
                    var paramsDto = new DownloadFileParamDto(detail, downloadFilesParam.DestFolder, downloadFilesParam.ApplicationFolder, downloadFilesParam.OfferFolder);
                    var result = await DownloadFile(paramsDto);
                    if (result != null && result.Value != null) listResult.Add(result.Value);
                }
            }
            return await Task.FromResult(new Result<string[]>(listResult.ToArray()));
        }

        public async Task<Result<FileDetails>> UploadFile(UploadFileParamDto uploadFileParam)
        {
            return await Task.Run(() =>
            {
                var file = new FileInfo(uploadFileParam.FilePath);
                try
                {
                    var hash = _hashService?.CalculateHash(file.FullName);
                    var destinationFileName = PathFactory.GetUploadDestinationFile(file, uploadFileParam.UploadFileDetails.DestFolder);
                    var keyRole = _keyProvider?.ProvideKey(uploadFileParam.UploadFileDetails.Role);

                    var name = file.Name;
                    if (name.Contains('.'))
                    {
                        name = name.Replace(file.Extension, "");
                    }
                    var details = new FileDetails(
                        name,
                        file.Extension,
                        uploadFileParam.UploadFileDetails.Role,
                        hash!,
                        file.Length,
                        uploadFileParam.UploadFileDetails.Category,
                        uploadFileParam.UploadFileDetails.Tags);
                    Log.Logger.Information(
                        $"details: \nname: {details.Name} \nRealExtension: {details.RealExtension}\nhash: {details.Hash}\nrole: {details.Role}\nsize: {details.Size}\n\n\n");

                    EncryptedCopyFile(file, destinationFileName, keyRole!);
                    var detailsPath = @$"{uploadFileParam.UploadFileDetails.DestFolder}\{details.NameExt}{ConstantValues.OfferFileExtension}{ConstantValues.OfferFileDetailsExtension}";
                    _fileOfferSource?.Save(details, detailsPath);
                    return new Result<FileDetails>(details);
                }
                catch (IOException)
                {
                    return new Result<FileDetails>(null, new[] { new Error($"Cannot upload the file [{file}] besouse is used by another process.") });
                }
                catch (Exception)
                {
                    return new Result<FileDetails>(null, new[] { new Error("no details") });
                }
            });
        }

        public async Task<Result<FileDetails[]>> UploadFiles(UploadFilesParamDto uploadFilesParam)
        {
            var list = new ConcurrentBag<FileDetails>();
            var errors = new ConcurrentBag<Error>();

            var tasks = uploadFilesParam.FilePaths.Select(file => Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        var uploadParams = new UploadFileParamDto(file, uploadFilesParam.UploadFileDetails, uploadFilesParam.ApplicationFolder, uploadFilesParam.OfferFolder);
                        var details = await UploadFile(uploadParams);
                        if (details != null && details.Errors == null)
                        {
                            list.Add(details.Value);
                        }
                        if (details != null && details.Errors != null && details.Errors.Length > 0)
                        {
                            errors.Add(details.Errors[0]);
                        }
                    }
                    catch (IOException)
                    {
                        errors.Add(new Error($"Cannot upload the file [{file}] besouse is used by another process."));
                    }
                }).Unwrap()).ToList();

            await Task.WhenAll(tasks);

            return new Result<FileDetails[]>(list.ToArray(), errors.ToArray());
        }

        private async Task<IEnumerable<string>?> GetFiles(string offerDir)
        {
            return await Task.Run(async () =>
            {
                var di = new DirectoryInfo(offerDir);
                return await di.WalkDirectoryTreeAsync("", false);
            });
        }

        private void EncryptedCopyFile(FileInfo file, string destinationFileName, IKey keyRole)
        {
            using var sourceStream = File.OpenRead(file.FullName);
            using var destinationStream = File.Create(destinationFileName);
            using var provider = _keyProvider?.ProvideCryptoService(keyRole);
            using var cryptoTransform = provider.CreateEncryptor();
            using var cryptoStream = new CryptoStream(destinationStream, cryptoTransform, CryptoStreamMode.Write);
            destinationStream.Write(provider.IV, 0, provider.IV.Length);
            sourceStream.CopyTo(cryptoStream);
        }

        public async Task<FileDetails?> GetFileDetail(string filePath) => await _fileOfferSource!.Get(filePath);
    }
}
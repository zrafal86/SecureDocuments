using SecureDocuments.Models;
using SecureDocuments.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;

namespace SecureDocuments.ViewModels
{
    public sealed class OfferFileListItemAdapter
    {
        public OfferFileListItemAdapter(string offerFolder, IDialogService? dialogService = null)
        {
            ShowRemovePopupCommand = ReactiveCommand.CreateFromTask<FileDetails, Unit>(async (details) =>
            {
                return await CheckPermission(Role.Manager, ShowRemovePopup, details);
            });
            ShowOpenPopupCommand = ReactiveCommand.CreateFromTask<FileDetails, Unit>(OpenFileAction);
            DialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            OfferFolder = offerFolder;
            FilesService = Locator.Current.GetService<IFilesService>();
            RoleAccessService = Locator.Current.GetService<IRoleAccessService>();
            UserSettings = Locator.Current.GetService<IUserSettings>();
        }

        [Reactive] public FileDetails? FileDetails { get; set; }
        [Reactive] public bool Selected { get; set; }
        public IRoleAccessService RoleAccessService { get; init; }
        public IUserSettings UserSettings { get; init; }
        public IDialogService? DialogService { get; init; }
        public string OfferFolder { get; }
        public IFilesService FilesService { get; init; }
        public ReactiveCommand<FileDetails, Unit> ShowRemovePopupCommand { get; }
        public ReactiveCommand<FileDetails, Unit> ShowOpenPopupCommand { get; }
        public Offer Offer { get; init; }
        public FolderPurpose FolderPurpose { get; init; }

        private async Task<Unit> OpenFileAction(FileDetails details)
        {
            var filesDir = PathFactory.GetDownloadDestinationFile(FolderPurpose, Offer);//TODO: make sure it is right path
            var downloadParam = new DownloadFileParamDto(details, filesDir, UserSettings.ApplicationFolder, OfferFolder);
            var result = await FilesService.DownloadFile(downloadParam);
            if (result != null && result.Errors == null)
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo($"{Path.Combine(filesDir, $"{details.Name}{details.RealExtension}")}")
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            else
            {
                await DialogService!.ShowDialog(
                    "Cannot open file",
                    $"Error occurred during opening file '{details.Name}{details.RealExtension} > {filesDir}'?");
            }
            return await Task.FromResult(Unit.Default);
        }

        public async Task<Unit> ShowRemovePopup(FileDetails details)
        {
            if (DialogService != null)
            {
                var fullPath = OfferFolder + Path.DirectorySeparatorChar + details.NameExt + ConstantValues.OfferFileExtension; //TODO: make sure it is right path
                var result = await DialogService.ShowDialog(
                    "Confirmation",
                    $"Are you sure that you want to delete this file '{details.Name}{details.RealExtension}'?",
                    true);
                if (result != null && result is bool success && success)
                {
                    try
                    {
                        File.Delete(fullPath);
                        File.Delete(fullPath + ConstantValues.OfferFileDetailsExtension);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message, e);
                        await DialogService.ShowDialog("Error", $"Cannot remove file: {fullPath}");
                    }
                }
            }

            return await Task.FromResult(Unit.Default);
        }

        private async Task<TResult?> CheckPermission<TParam, TResult>(Role role, Func<TParam, Task<TResult>> func, TParam param)
        {
            var result = RoleAccessService.CheckAccess(role);
            if (!result)
            {
                await ShowNoPermissionDialog();
                return default;
            }
            else
            {
                return await func.Invoke(param);
            }
        }

        private async Task ShowNoPermissionDialog() => await DialogService!
            .ShowDialog("No permission", "You do not have sufficient permissions for this action?");

    }
}
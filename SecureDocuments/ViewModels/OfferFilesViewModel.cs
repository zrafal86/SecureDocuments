using SecureDocuments.Factories;
using SecureDocuments.Helpers;
using SecureDocuments.Models;
using SecureDocuments.Models.Common;
using SecureDocuments.Models.File;
using SecureDocuments.Services;
using SecureDocuments.Services.Hash;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;

namespace SecureDocuments.ViewModels
{
    internal record RefreshDialogProperties
    {
        public string? DialogId { get; init; }
        public bool IsVisible { get; init; }
    }

    public sealed class OfferFilesViewModel : ViewModelBase
    {
        private readonly IFilesService _fileService;
        private readonly ICategoryNamesSource _categoryNameSource;
        private readonly Offer _currentOffer;
        private readonly FolderPurpose _purpose;

        public OfferFilesViewModel(
            Offer offer,
            FolderPurpose folderPurpose,
            IDialogService dialogService,
            IScreen? screen = null) : base(screen, dialogService)
        {
            _currentOffer = offer;
            _purpose = folderPurpose;
            OfferFolder = _purpose switch //TODO: offer folder located here
            {
                FolderPurpose.Normal => PathFactory.GetOfferDirectoryPath(offer)!,
                FolderPurpose.Invoices => PathFactory.GetOfferInvoicesDirectoryPath(offer)!,
                _ => throw new NotImplementedException(),
            };
            _fileService = Locator.Current.GetService<IFilesService>() ?? throw new ArgumentException("Cannot find file service. Make sure that it is registered."); ;
            _categoryNameSource = Locator.Current.GetService<ICategoryNamesSource>() ?? throw new ArgumentException("Cannot find category names. Make sure that it is registered in container."); ;
            UploadFilesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                _ = await CheckPermission(Role.Technologist, async () => await UploadFiles());
            });
            DownloadFilesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                _ = await CheckPermission(Role.Reader, async () => await DownloadFiles());
            });
            SyncFilesCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                _ = await CheckPermission(Role.Technologist, async () => await SyncFiles());
            });
            OpenFilesFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CheckPermission(Role.Reader, OpenFilesFolder);
            });
            RefreshOfferFilesCommand = ReactiveCommand.CreateFromTask(GetOfferFiles);


            var categories = _categoryNameSource.GetCategories(folderPurpose);
            categories.Insert(0, new CategoryNameLookup(CategoryName.All));
            FileCategories = new ObservableCollection<CategoryNameLookup>(categories);

            _ = RefreshOfferFilesCommand.IsExecuting
                .Skip(1)
                .Where(isExecuting => !isExecuting)
                .Subscribe(_ =>
                {
                    if (RefreshDialog != null && RefreshDialog.IsVisible)
                    {
                        _dialogService.Close(RefreshDialog.DialogId!);
                    }
                });
        }

        public void SelectionChanged(List<OfferFileListItemAdapter> list)
        {
            SelectedItems = list;
            if (list != null && list.Count > 0)
            {
                UpdateTitleText(list.Count);
            }
        }

        public override string UrlPathSegment => "Offer files";

        [Reactive] public string TitleText { get; set; } = "";
        [Reactive] public string SearchTerm { get; set; } = "";
        [Reactive] public string OfferFolder { get; set; } = "";
        [Reactive] public CategoryName SelectedCategory { get; set; } = CategoryName.All;

        public ReactiveCommand<Unit, Unit> UploadFilesCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadFilesCommand { get; }
        public ReactiveCommand<Unit, Unit> SyncFilesCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenFilesFolderCommand { get; }
        public ReactiveCommand<Unit, IEnumerable<OfferFileListItemAdapter>> RefreshOfferFilesCommand { get; }

        private SourceList<OfferFileListItemAdapter> _offerFiles { get; } = new SourceList<OfferFileListItemAdapter>();

        public ReadOnlyObservableCollection<OfferFileListItemAdapter> OfferFiles;

        public ObservableCollection<CategoryNameLookup> FileCategories { get; }
        public ObservableCollection<RoleLookup> Roles { get; }
        internal RefreshDialogProperties RefreshDialog { get; private set; }
        public List<OfferFileListItemAdapter> SelectedItems { get; set; }
        public int OfferFilesCount { get; private set; }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --Offer files-- screen!");
            RefreshOfferFiles(true).ContinueWith(t => { Log.Error(t.Exception?.Message, t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);

            var watchedFiles = new FileWatcherHelper().WatchFilesInOfferFolder(OfferFolder);
            _ = watchedFiles
                .Subscribe(async watchedFile =>
                {
                    await FileChanged(watchedFile);
                }, exception => Log.Error(exception.Message, exception)
                ).DisposeWith(disposables);

            _ = _offerFiles.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out OfferFiles)
                .DisposeMany()
                .Subscribe();

            var subscription = _offerFiles.CountChanged.ObserveOn(RxApp.MainThreadScheduler).Subscribe(count =>
            {
                OfferFilesCount = count;
                UpdateTitleText(0);
            });
            disposables.Add(subscription);

            this.WhenAnyValue(vm => vm.SearchTerm)
                .Skip(1)
                .Throttle(TimeSpan.FromSeconds(2))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async searchTerm => await SearchFilesWith(searchTerm))
                .DisposeWith(disposables);

            this.WhenAnyValue(vm => vm.SelectedCategory)
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async category => await ShowFilesWith(category))
                .DisposeWith(disposables);
        }

        private void UpdateTitleText(int selectedItemsCount = 0)
        {
            TitleText = $"{_currentOffer.Name} [{_currentOffer.Manager?.FullName ?? ""}] ({OfferFilesCount} files) - selected items : {selectedItemsCount}";
        }

        private async Task FileChanged(WatchedFile watchedFile)
        {
            switch (watchedFile.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    if (watchedFile != null && watchedFile.FullPath != null && watchedFile.FullPath.EndsWith(".details"))
                    {
                        var detail = await _fileService.GetFileDetail(watchedFile.FullPath);
                        _offerFiles.Add(new OfferFileListItemAdapter(OfferFolder)
                        {
                            FileDetails = detail,
                            Selected = false,
                            Offer = _currentOffer,
                            FolderPurpose = _purpose
                        });
                    }
                    break;
                case WatcherChangeTypes.Deleted:
                    var toRemove = OfferFiles.ToList().Where(f =>
                        f.FileDetails != null &&
                        watchedFile.Name != null &&
                        watchedFile.Name.StartsWith(f.FileDetails.Name))
                    .FirstOrDefault();
                    if (toRemove != null) { _ = _offerFiles.Remove(toRemove); }
                    break;
                case WatcherChangeTypes.Changed:
                    if (watchedFile != null && watchedFile.FullPath != null && watchedFile.FullPath.EndsWith(".details"))
                    {
                        var details = await _fileService.GetFileDetail(watchedFile.FullPath);
                        var detailsAdapter = new OfferFileListItemAdapter(OfferFolder)
                        {
                            FileDetails = details,
                            Selected = false,
                            FolderPurpose = _purpose,
                            Offer = _currentOffer
                        };
                        _offerFiles.Edit(updater =>
                        {
                            var toUpdate = updater.Where(x => x.FileDetails!.Name.Equals(@$"\{details!.Name}")).FirstOrDefault();
                            if (toUpdate != null)
                            {
                                updater.Replace(toUpdate, detailsAdapter);
                            }
                        });
                    }
                    break;
                case WatcherChangeTypes.Renamed:
                    await RefreshOfferFiles(false);
                    break;
                case WatcherChangeTypes.All:
                    await RefreshOfferFiles(false);
                    break;
                default:
                    await RefreshOfferFiles(false);
                    break;
            }
        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --Offer files-- screen!");
            _offerFiles.Edit(updater => updater.Clear());
            // MessageBus.Current.SendMessage(new RefreshActionEvent { Id = _currentOffer.Id! }); //TODO: it does not work as expected
        }

        private async Task SearchFilesWith(string searchTerm)
        {
            var id = await _dialogService.ShowWaitDialog();
            Log.Debug($"searchTerm: {searchTerm}");
            var files = await GetOfferFiles();
            if (SelectedCategory != CategoryName.All)
                files = files
                    .Where(fileAdapter => fileAdapter.FileDetails?.Category == SelectedCategory)
                    .ToList();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTerms = searchTerm.ToLower().Split(',');
                var result = SearchForTerms(files.Select(f => f.FileDetails), searchTerms);
                if (result != null) files = result;
            }
            _dialogService.Close(id);
            UpdateList(files);
        }

        private IEnumerable<OfferFileListItemAdapter> SearchForTerms(IEnumerable<FileDetails?> files, string[] searchTerms)
        {
            var fileDetails = files.Where(file => file != null).Select(file => file!);

            var result = searchTerms.Aggregate(fileDetails,
                (current, term) => current.Where(fileDetail =>
                    fileDetail.Category.ToString().ToLower().Contains(term) ||
                    fileDetail.Role.ToString().Contains(term) ||
                    fileDetail.Name.ToLower().Contains(term) ||
                    fileDetail.RealExtension.ToLower().Contains(term) ||
                    fileDetail.Tags != null && fileDetail.Tags
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t.Trim().ToLower())
                        .ToList().Contains(term)
                ));

            return MapFileDetailsIntoListAdapter(result);
        }

        private async Task ShowFilesWith(CategoryName category)
        {
            SearchTerm = "";
            var id = await _dialogService.ShowWaitDialog();
            var files = await GetOfferFiles();
            var filesDetail = files.Select(f => f.FileDetails);
            if (category != CategoryName.All && filesDetail != null)
                filesDetail = filesDetail
                    .Where(x => x != null)
                    .Where(x => x?.Category == category).ToList();
            _dialogService.Close(id);
            if (filesDetail != null)
            {
                var list = MapFileDetailsIntoListAdapter(filesDetail!);
                UpdateList(list);
            }
        }

        private void UpdateList(IEnumerable<OfferFileListItemAdapter> list)
        {
            _offerFiles.Edit(updater =>
            {
                updater.Clear();
                updater.AddRange(list);
            });
        }

        private async Task<IEnumerable<OfferFileListItemAdapter>> GetOfferFiles()
        {
            var result = await _fileService.GetOfferFiles(OfferFolder);
            var filteredResult = result.Where(file => _roleAccessService.CheckAccess(file.Role));
            return MapFileDetailsIntoListAdapter(filteredResult);
        }

        private IEnumerable<OfferFileListItemAdapter> MapFileDetailsIntoListAdapter(IEnumerable<FileDetails>? filesDetail)
        {
            if (filesDetail != null)
            {
                return filesDetail.Select(file => new OfferFileListItemAdapter(OfferFolder)
                {
                    FileDetails = file,
                    Selected = false,
                    DialogService = _dialogService,
                    Offer = _currentOffer,
                    FolderPurpose = _purpose,

                });
            }
            else
            {
                return Enumerable.Empty<OfferFileListItemAdapter>();
            }
        }

        private async Task RefreshOfferFiles(bool showWaitDialog = true)
        {
            if (showWaitDialog)
            {
                var waitIdDialog = await _dialogService.ShowWaitDialog();
                RefreshDialog = new RefreshDialogProperties { DialogId = waitIdDialog, IsVisible = true };
            }
            else
            {
                RefreshDialog = new RefreshDialogProperties { DialogId = null, IsVisible = false };
            }
            RefreshOfferFilesCommand.Execute().Subscribe(offerFiles =>
            {
                UpdateList(offerFiles);
            });
        }

        private async Task<Unit> UploadFiles() //choose category and role
        {
            var view = Locator.Current.GetService<IViewFor<AddFilesDialogViewModel>>();
            object? resultDialog = null;
            view!.ViewModel ??= new AddFilesDialogViewModel(_purpose);
            resultDialog = await _dialogService.ShowDialog(view);

            if (resultDialog is not null && resultDialog is bool success && success)
            {
                var identifier = await _dialogService.ShowWaitDialog();
                CreateUploadFileDetails(
                    view,
                    out string[] selectedFiles,
                    out var category,
                    out var role,
                    out UploadFileDetails uploadFileDetails);

                if (category == CategoryName.All || role == Role.Creator)
                {
                    _dialogService.Close(identifier);
                    await _dialogService.ShowDialog("Invalid data",
                        "Please provide correct values for role, category, and files.");
                    return await Task.FromResult(Unit.Default);
                }

                var uploadParams = new UploadFilesParamDto(selectedFiles, uploadFileDetails, _userSettings?.ApplicationFolder!, OfferFolder);
                var result = await Task.Run(async () => await _fileService.UploadFiles(uploadParams));

                _dialogService.Close(identifier);

                if (result != null && result.Errors != null && result.Errors.Length > 0)
                {
                    SelectedCategory = CategoryName.All;

                    var errors = result.Errors;
                    var sb = new StringBuilder();
                    int i = 0;
                    foreach (var error in errors)
                    {
                        var errorText = error.Msg;
                        sb.Append(++i).Append(".\t");
                        sb.Append(errorText);
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    sb.Append("Make sure all open documents are closed properly.");
                    sb.AppendLine();
                    await _dialogService.ShowDialog("Error occurred during uploading files.", sb.ToString(), false);
                }

            }

            return await Task.FromResult(Unit.Default);
        }

        private void CreateUploadFileDetails(
            IViewFor<AddFilesDialogViewModel> view,
            out string[] selectedFiles,
            out CategoryName category,
            out Role role,
            out UploadFileDetails uploadFileDetails)
        {
            selectedFiles = view.ViewModel.Files.ToArray();
            category = view.ViewModel.SelectedCategory;
            role = view.ViewModel.SelectedRole;
            var tags = view.ViewModel.Tags;
            uploadFileDetails = new UploadFileDetails(
                OfferFolder,
                role,
                category,
                tags.Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray());
        }

        private async Task<Unit> DownloadFiles()
        {
            var destFolder = PathFactory.GetDownloadDestinationFile(_purpose, _currentOffer);
            var selectedFiles = SelectedItems?.Where(x => x.FileDetails != null).Select(x => x.FileDetails!).ToArray();
            if (selectedFiles != null && selectedFiles.Length > 0)
            {
                var identifier = await _dialogService.ShowWaitDialog();
                var result = await Task.Run(async () =>
                {
                    var downloadFilesParam = new DownloadFilesParamDto(selectedFiles, destFolder, _userSettings?.ApplicationFolder!, OfferFolder);
                    return await _fileService.DownloadFiles(downloadFilesParam);
                });
                _dialogService.Close(identifier);
                RxApp.MainThreadScheduler.Schedule(() => Process.Start("explorer.exe", destFolder));
                return await Task.FromResult(Unit.Default);
            }

            await _dialogService.ShowDialog("No files to download", "First you need to select some files.");
            return await Task.FromResult(Unit.Default);
        }

        private async Task<Unit> SyncFiles()
        {
            var identifier = await _dialogService.ShowWaitDialog();
            var result = await Task.Run(async () => await SyncAllLocalFilesAction());

            var notSyncFiles = result.Where(x => x.Errors != null && x.Errors.Length > 0);

            var sb = new StringBuilder();
            int i = 0;
            foreach (var item in notSyncFiles)
            {
                var errorText = item.Errors?[0].Msg;
                sb.Append(++i).Append(".\t");
                sb.Append(errorText);
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append("Make sure all open documents are closed properly.");
            sb.AppendLine();
            _dialogService.Close(identifier);
            if (notSyncFiles.Any())
            {
                await _dialogService.ShowDialog("Error occurred during synchronization.", sb.ToString(), false);
            }

            await RefreshOfferFiles(false);
            return await Task.FromResult(Unit.Default);
        }

        private void OpenFilesFolder()
        {
            var destFolder = PathFactory.GetDownloadDestinationFile(_purpose, _currentOffer);
            Process.Start("explorer.exe", destFolder);
        }

        private async Task<ImmutableArray<Result<string>>> SyncAllLocalFilesAction()
        {
            var localFolder = PathFactory.GetDownloadDestinationFile(_purpose, _currentOffer);
            Log.Debug("SyncFiles");
            Log.Debug($"Server OfferFolder : {OfferFolder}");
            Log.Debug($"Local OfferFolder : {localFolder}");

            var networkFiles = OfferFiles.ToList();

            var list = new ConcurrentBag<Result<string>>();

            var tasks = networkFiles.Select(file => Task.Factory.StartNew(
                async () =>
                {
                    var details = file.FileDetails;
                    var localFile = Path.Combine(localFolder, details?.NameExt ?? "");
                    try
                    {
                        var fi = new FileInfo(localFile);
                        if (fi.Exists)
                        {
                            var hashCalculator = Locator.Current.GetService<IHashCalculator>();
                            var localHash = hashCalculator?.CalculateHash(localFile);
                            if (!details!.Hash.Equals(localHash))
                            {
                                var localModTime = fi.LastWriteTime;
                                var netFi = new FileInfo(@$"{OfferFolder}\{details.NameExt}\{ConstantValues.OfferFileExtension}");
                                var netModTime = netFi.LastWriteTime;
                                if (localModTime > netModTime)
                                {
                                    var ufd = new UploadFileDetails(OfferFolder, details.Role, details.Category, details.Tags ?? Array.Empty<string>());
                                    var ufdParams = new UploadFileParamDto(localFile, ufd, _userSettings?.ApplicationFolder!, OfferFolder);
                                    await _fileService.UploadFile(ufdParams);
                                }
                                else
                                {
                                    var downloadFileParams = new DownloadFileParamDto(details, localFolder, _userSettings?.ApplicationFolder!, OfferFolder);
                                    _ = await _fileService.DownloadFile(downloadFileParams);
                                }
                            }
                        }
                    }
                    catch (IOException e)
                    {
                        list.Add(new Result<string>(localFile, new Error[] { new Error($"Cannot synchronize file [{localFile}] besouse is used by another process.", e) }));
                    }
                }).Unwrap()).ToList();

            await Task.WhenAll(tasks);

            return list.ToImmutableArray();
        }
    }
}
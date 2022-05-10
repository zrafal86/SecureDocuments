using SecureDocuments.Factories;
using SecureDocuments.Models;
using SecureDocuments.Models.File;
using SecureDocuments.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;

namespace SecureDocuments.ViewModels
{
    public sealed class AddFilesDialogViewModel : ViewModelBase
    {
        private readonly IFileChooser _fileChooser;
        private readonly ICategoryNamesSource _categoryNameSource;

        public AddFilesDialogViewModel(FolderPurpose purpose, IFileChooser? fileChooser = null)
        {
            _fileChooser = fileChooser ?? Locator.Current.GetService<IFileChooser>();
            _categoryNameSource = Locator.Current.GetService<ICategoryNamesSource>();
            Files = new ObservableCollection<string>(Enumerable.Empty<string>());
            ChooseFilesCommand = ReactiveCommand.CreateFromTask(ChooseFiles);

            var categories = _categoryNameSource.GetCategories(purpose);
            FileCategories = new ObservableCollection<CategoryNameLookup>(categories);

            var roles = ((IList<Role>)Enum.GetValues(typeof(Role)))
                .Where(role => role != Role.Creator)
                .Select(role => new RoleLookup(role));
            Roles = new ObservableCollection<RoleLookup>(roles);
        }

        public override string UrlPathSegment => "";
        public ICommand ChooseFilesCommand { get; }
        public ObservableCollection<string> Files { get; }
        public ObservableCollection<CategoryNameLookup> FileCategories { get; }
        public ObservableCollection<RoleLookup> Roles { get; }
        [Reactive] public CategoryName SelectedCategory { get; set; }
        [Reactive] public Role SelectedRole { get; set; }
        [Reactive] public string Tags { get; set; }

        private async Task<Unit> ChooseFiles()
        {
            var files = _fileChooser.ChooseFiles();
            Files.Clear();
            Files.AddRange(files);
            return await Task.FromResult(Unit.Default);
        }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --AddFilesDialog-- screen!");

        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --AddFilesDialog-- screen!");
        }
    }
}
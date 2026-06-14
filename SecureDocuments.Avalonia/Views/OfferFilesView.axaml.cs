using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.ViewModels;
using SecureDocuments.Avalonia.Extensions;

namespace SecureDocuments.Avalonia.Views;

public partial class OfferFilesView : ReactiveUserControl<OfferFilesViewModel>
{
    public OfferFilesView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.SearchTerm, view => view.SearchTextBox.Text)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.FileCategories, view => view.FileCategoriesComboBox.ItemsSource)
                .DisposeWith(disposables);
            FileCategoriesComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedCategory,
                view => view.FileCategoriesComboBox.SelectedItem,
                category => ViewModel?.FileCategories?.FirstOrDefault(l => l.Item.Equals(category)),
                item => (item as CategoryNameLookup)?.Item ?? CategoryName.All)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.TitleText, view => view.OfferFilesTitleText.Text)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.OfferFiles, view => view.OfferFilesListBox.ItemsSource)
                .DisposeWith(disposables);

            OfferFilesListBox.SelectionChanged<OfferFileListItemAdapter>()
                .Subscribe(list => ViewModel?.SelectionChanged(list.ToList()))
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.UploadFilesCommand, view => view.UploadFiles)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.DownloadFilesCommand, view => view.DownloadFiles)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SyncFilesCommand, view => view.SyncFiles)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.OpenFilesFolderCommand, view => view.OpenFilesFolder)
                .DisposeWith(disposables);
        });
    }
}

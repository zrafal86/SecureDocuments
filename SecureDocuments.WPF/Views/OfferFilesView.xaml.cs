using SecureDocuments.ViewModels;
using SecureDocuments.WPF.Extensions;
using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for OfferFilesView.xaml
    /// </summary>
    public partial class OfferFilesView
    {
        public OfferFilesView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                        viewModel => viewModel.SearchTerm,
                        view => view.SearchTextBox.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedCategory,
                        view => view.FileCategoriesComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.TitleText,
                        view => view.OfferFilesTitleText.Text)
                    .DisposeWith(disposables);

                OfferFilesListView.SelectionChanged<OfferFileListItemAdapter>()
                    .Do(list => ViewModel!.SelectionChanged(list))
                    .SubscribeOn(RxApp.MainThreadScheduler)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe()
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.OfferFiles,
                        view => view.OfferFilesListView.ItemsSource)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.FileCategories,
                        view => view.FileCategoriesComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.UploadFilesCommand,
                        view => view.UploadFiles)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.DownloadFilesCommand,
                        view => view.DownloadFiles)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.SyncFilesCommand,
                        view => view.SyncFiles)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.OpenFilesFolderCommand,
                        view => view.OpenFilesFolder)
                    .DisposeWith(disposables);
            });
        }
    }
}
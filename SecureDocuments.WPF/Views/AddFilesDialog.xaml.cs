using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for AddFilesDialog.xaml
    /// </summary>
    public partial class AddFilesDialog
    {
        public AddFilesDialog()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                        viewModel => viewModel.Tags,
                        view => view.TagsTextBox.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedCategory,
                        view => view.FileCategoryComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedRole,
                        view => view.RoleComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        vm => vm.ChooseFilesCommand,
                        v => v.ChooseFilesButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Roles,
                        view => view.RoleComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.FileCategories,
                        view => view.FileCategoryComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Files,
                        view => view.FilesListBox.ItemsSource)
                    .DisposeWith(disposables);
            });
        }
    }
}
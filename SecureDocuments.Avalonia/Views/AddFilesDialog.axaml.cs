using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.Models;
using SecureDocuments.Models.File;
using SecureDocuments.ViewModels;

namespace SecureDocuments.Avalonia.Views;

public partial class AddFilesDialog : ReactiveUserControl<AddFilesDialogViewModel>
{
    public AddFilesDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.Tags, view => view.TagsTextBox.Text)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.Roles, view => view.RoleComboBox.ItemsSource)
                .DisposeWith(disposables);
            RoleComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedRole,
                view => view.RoleComboBox.SelectedItem,
                role => ViewModel?.Roles?.FirstOrDefault(l => l.Item.Equals(role)),
                item => (item as RoleLookup)?.Item ?? default)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.FileCategories, view => view.FileCategoryComboBox.ItemsSource)
                .DisposeWith(disposables);
            FileCategoryComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedCategory,
                view => view.FileCategoryComboBox.SelectedItem,
                category => ViewModel?.FileCategories?.FirstOrDefault(l => l.Item.Equals(category)),
                item => (item as CategoryNameLookup)?.Item ?? default)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.Files, view => view.FilesListBox.ItemsSource)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.ChooseFilesCommand, view => view.ChooseFilesButton)
                .DisposeWith(disposables);
        });
    }
}

using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.Models;
using SecureDocuments.ViewModels;

namespace SecureDocuments.Avalonia.Views;

public partial class NewOfferView : ReactiveUserControl<NewOfferViewModel>
{
    public NewOfferView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.OfferTypes, view => view.TypeComboBox.ItemsSource)
                .DisposeWith(disposables);
            TypeComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedOfferType,
                view => view.TypeComboBox.SelectedItem,
                offerType => ViewModel?.OfferTypes?.FirstOrDefault(l => l.Item.Equals(offerType)),
                item => (item as OfferTypeLookup)?.Item ?? default)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.Managers, view => view.ManagerComboBox.ItemsSource)
                .DisposeWith(disposables);
            ManagerComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedManager,
                view => view.ManagerComboBox.SelectedItem,
                user => ViewModel?.Managers?.FirstOrDefault(l => l.Item?.Email == user?.Email),
                item => (item as UserLookup)?.Item)
                .DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.OfferName, view => view.OfferName.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.OfferDescription, view => view.OfferDescription.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CustomerName, view => view.CustomerName.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CustomerDescription, view => view.CustomerDescription.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.CancelCommand, view => view.CancelButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddCommand, view => view.AddButton)
                .DisposeWith(disposables);
        });
    }
}

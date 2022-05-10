using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for NewOfferView.xaml
    /// </summary>
    public partial class NewOfferView
    {
        public NewOfferView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.OfferTypes,
                        view => view.TypeComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Managers,
                        view => view.ManagerComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedOfferType,
                        view => view.TypeComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedManager,
                        view => view.ManagerComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.OfferName,
                        view => view.OfferName.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.OfferDescription,
                        view => view.OfferDescription.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.CustomerName,
                        view => view.CustomerName.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.CustomerDescription,
                        view => view.CustomerDescription.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.CancelCommand,
                        view => view.CancelButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.AddCommand,
                        view => view.AddButton)
                    .DisposeWith(disposables);
            });
        }
    }
}
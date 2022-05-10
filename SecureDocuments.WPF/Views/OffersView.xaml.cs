using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    public partial class OffersView
    {
        public OffersView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedStatus,
                        view => view.StatusComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SearchTerm,
                        view => view.SearchTextBox.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedOffer,
                        view => view.OffersListView.SelectedItem)
                    .DisposeWith(disposables);

                //DetailsPanel Form
                this.Bind(ViewModel,
                        viewModel => viewModel.OfferNumber,
                        view => view.OfferNumber.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.OfferName,
                        view => view.OfferName.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.CustomerName,
                        view => view.CustomerName.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CustomerCountries,
                        view => view.CustomerCountryComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedCustomerCountry,
                        view => view.CustomerCountryComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.CustomerAddress,
                        view => view.CustomerAddress.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.CustomerDescription,
                        view => view.CustomerDescription.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.OfferDescription,
                        view => view.OfferDescription.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.InitialGrossAmount,
                        view => view.InitialGrossAmount.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.UnitFlagCountries,
                        view => view.UnitFlagComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedUnitFlagCountry,
                        view => view.UnitFlagComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CurrencySymbolsCollection,
                        view => view.CurrencyComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedCurrency,
                        view => view.CurrencyComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.OfferTypes,
                        view => view.TypeComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedOfferType,
                        view => view.TypeComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Applicants,
                        view => view.ApplicantComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedApplicant,
                        view => view.ApplicantComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Managers,
                        view => view.ManagerComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedManager,
                        view => view.ManagerComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Builders,
                        view => view.BuilderComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedBuilder,
                        view => view.BuilderComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Technologists,
                        view => view.TechnologistComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CompanyNames,
                        view => view.CompanyNameComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedCompanyName,
                        view => view.CompanyNameComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Subjects,
                        view => view.SubjectComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedSubject,
                        view => view.SubjectComboBox.SelectedValue)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.SelectedTechnologist,
                        view => view.TechnologistComboBox.SelectedValue)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                        viewModel => viewModel.StartDate,
                        view => view.StartDate.SelectedDate)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.RealEndDate,
                        view => view.EndDate.SelectedDate)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.EstimatedEndDate,
                        view => view.EstimatedEndDate.SelectedDate)
                    .DisposeWith(disposables);
                this.Bind(ViewModel,
                        viewModel => viewModel.AcceptedDate,
                        view => view.AcceptedDate.Content)
                    .DisposeWith(disposables);
                //End Details Form

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Offers,
                        view => view.OffersListView.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.Statuses,
                        view => view.StatusComboBox.ItemsSource)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.SelectedOffer,
                        view => view.DetailsPanel.Visibility,
                        x => x != null ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.HasManagerAccess,
                        view => view.InitialGrossAmountParent.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.HasManagerAccess,
                        view => view.ExportOffersButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.HasManagerAccess,
                        view => view.ApplicantLabel.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.HasManagerAccess,
                        view => view.ApplicantComboBox.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                        viewModel => viewModel.HasManagerAccess,
                        view => view.AddOfferButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CanAccept,
                        view => view.AcceptButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CanReject,
                        view => view.RejectButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CanFinish,
                        view => view.FinishButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel,
                        viewModel => viewModel.CanArchive,
                        view => view.ArchiveButton.Visibility, x => x ? Visibility.Visible : Visibility.Collapsed)
                    .DisposeWith(disposables);


                this.BindCommand(ViewModel,
                        viewModel => viewModel.AddOfferCommand,
                        view => view.AddOfferButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.ExportOfferDataToFileCommand,
                        view => view.ExportOffersButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.RefreshOffersCommand,
                        view => view.RefreshOffersButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.AddFileToOfferCommand,
                    view => view.AddOfferFileButton,
                    viewModel => viewModel.SelectedOffer)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        viewModel => viewModel.AddInvoicesFileCommand,
                        view => view.AddInvoicesFileButton,
                        viewModel => viewModel.SelectedOffer)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.SaveOfferCommand,
                        view => view.SaveOfferFileButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.CloseDetailsPanelCommand,
                        view => view.CloseDetailsPanelButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.AcceptCommand,
                        view => view.AcceptButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.RejectCommand,
                        view => view.RejectButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.FinishCommand,
                        view => view.FinishButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel,
                        viewModel => viewModel.ArchiveCommand,
                        view => view.ArchiveButton)
                    .DisposeWith(disposables);
            });
        }
    }
}
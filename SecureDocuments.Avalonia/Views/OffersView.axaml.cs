using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.Models;
using SecureDocuments.ViewModels;

namespace SecureDocuments.Avalonia.Views;

public partial class OffersView : ReactiveUserControl<OffersViewModel>
{
    public OffersView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            // Status filter
            this.OneWayBind(ViewModel, vm => vm.Statuses, view => view.StatusComboBox.ItemsSource)
                .DisposeWith(disposables);
            StatusComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedStatus,
                view => view.StatusComboBox.SelectedItem,
                status => ViewModel?.Statuses?.FirstOrDefault(l => l.Item.Equals(status)),
                item => (item as StatusLookup)?.Item ?? Status.All)
                .DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.SearchTerm, view => view.SearchTextBox.Text)
                .DisposeWith(disposables);

            // Offers list
            this.OneWayBind(ViewModel, vm => vm.Offers, view => view.OffersDataGrid.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedOffer, view => view.OffersDataGrid.SelectedItem)
                .DisposeWith(disposables);

            // Details panel visibility
            this.OneWayBind(ViewModel,
                vm => vm.SelectedOffer,
                view => view.DetailsPanel.IsVisible,
                x => x != null)
                .DisposeWith(disposables);

            // Form fields
            this.Bind(ViewModel, vm => vm.OfferNumber, view => view.OfferNumber.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.OfferName, view => view.OfferName.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CustomerName, view => view.CustomerName.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CustomerAddress, view => view.CustomerAddress.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.OfferDescription, view => view.OfferDescription.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.CustomerDescription, view => view.CustomerDescription.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.InitialGrossAmount, view => view.InitialGrossAmount.Text)
                .DisposeWith(disposables);

            // Accepted date display
            this.OneWayBind(ViewModel,
                vm => vm.AcceptedDate,
                view => view.AcceptedDateText.Text,
                dt => dt == default ? "" : dt.ToString("yyyy-MM-dd"))
                .DisposeWith(disposables);

            // Offer type
            this.OneWayBind(ViewModel, vm => vm.OfferTypes, view => view.TypeComboBox.ItemsSource)
                .DisposeWith(disposables);
            TypeComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedOfferType,
                view => view.TypeComboBox.SelectedItem,
                offerType => ViewModel?.OfferTypes?.FirstOrDefault(l => l.Item.Equals(offerType)),
                item => (item as OfferTypeLookup)?.Item ?? default)
                .DisposeWith(disposables);

            // Subject
            this.OneWayBind(ViewModel, vm => vm.Subjects, view => view.SubjectComboBox.ItemsSource)
                .DisposeWith(disposables);
            SubjectComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedSubject,
                view => view.SubjectComboBox.SelectedItem,
                subject => ViewModel?.Subjects?.FirstOrDefault(l => l.Item == subject),
                item => (item as SubjectLookup)?.Item)
                .DisposeWith(disposables);

            // Company name
            this.OneWayBind(ViewModel, vm => vm.CompanyNames, view => view.CompanyNameComboBox.ItemsSource)
                .DisposeWith(disposables);
            CompanyNameComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedCompanyName,
                view => view.CompanyNameComboBox.SelectedItem,
                companyName => ViewModel?.CompanyNames?.FirstOrDefault(l => l.Item == companyName),
                item => (item as CompanyNameLookup)?.Item)
                .DisposeWith(disposables);

            // Customer country
            this.OneWayBind(ViewModel, vm => vm.CustomerCountries, view => view.CustomerCountryComboBox.ItemsSource)
                .DisposeWith(disposables);
            CustomerCountryComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedCustomerCountry,
                view => view.CustomerCountryComboBox.SelectedItem,
                country => ViewModel?.CustomerCountries?.FirstOrDefault(l => l.Item?.Id == country?.Id),
                item => (item as CustomerCountriesLookup)?.Item ?? default!)
                .DisposeWith(disposables);

            // Unit flag
            this.OneWayBind(ViewModel, vm => vm.UnitFlagCountries, view => view.UnitFlagComboBox.ItemsSource)
                .DisposeWith(disposables);
            UnitFlagComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedUnitFlagCountry,
                view => view.UnitFlagComboBox.SelectedItem,
                country => ViewModel?.UnitFlagCountries?.FirstOrDefault(l => l.Item?.Id == country?.Id),
                item => (item as CustomerCountriesLookup)?.Item ?? default!)
                .DisposeWith(disposables);

            // Currency
            this.OneWayBind(ViewModel, vm => vm.CurrencySymbolsCollection, view => view.CurrencyComboBox.ItemsSource)
                .DisposeWith(disposables);
            CurrencyComboBox.DisplayMemberBinding = new Binding("Item.ISOCurrencySymbol");
            this.Bind(ViewModel,
                vm => vm.SelectedCurrency,
                view => view.CurrencyComboBox.SelectedItem,
                currency => ViewModel?.CurrencySymbolsCollection?.FirstOrDefault(l => l.Item?.ISOCurrencySymbol == currency?.ISOCurrencySymbol),
                item => (item as CurrencySymbolLookup)?.Item ?? default!)
                .DisposeWith(disposables);

            // Applicant
            this.OneWayBind(ViewModel, vm => vm.Applicants, view => view.ApplicantComboBox.ItemsSource)
                .DisposeWith(disposables);
            ApplicantComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedApplicant,
                view => view.ApplicantComboBox.SelectedItem,
                user => ViewModel?.Applicants?.FirstOrDefault(l => l.Item?.Email == user?.Email),
                item => (item as UserLookup)?.Item)
                .DisposeWith(disposables);

            // Manager
            this.OneWayBind(ViewModel, vm => vm.Managers, view => view.ManagerComboBox.ItemsSource)
                .DisposeWith(disposables);
            ManagerComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedManager,
                view => view.ManagerComboBox.SelectedItem,
                user => ViewModel?.Managers?.FirstOrDefault(l => l.Item?.Email == user?.Email),
                item => (item as UserLookup)?.Item)
                .DisposeWith(disposables);

            // Builder
            this.OneWayBind(ViewModel, vm => vm.Builders, view => view.BuilderComboBox.ItemsSource)
                .DisposeWith(disposables);
            BuilderComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedBuilder,
                view => view.BuilderComboBox.SelectedItem,
                user => ViewModel?.Builders?.FirstOrDefault(l => l.Item?.Email == user?.Email),
                item => (item as UserLookup)?.Item)
                .DisposeWith(disposables);

            // Technologist
            this.OneWayBind(ViewModel, vm => vm.Technologists, view => view.TechnologistComboBox.ItemsSource)
                .DisposeWith(disposables);
            TechnologistComboBox.DisplayMemberBinding = new Binding("Display");
            this.Bind(ViewModel,
                vm => vm.SelectedTechnologist,
                view => view.TechnologistComboBox.SelectedItem,
                user => ViewModel?.Technologists?.FirstOrDefault(l => l.Item?.Email == user?.Email),
                item => (item as UserLookup)?.Item)
                .DisposeWith(disposables);

            // Date pickers (DateTimeOffset? ↔ DateTime)
            this.Bind(ViewModel,
                vm => vm.StartDate,
                view => view.StartDatePicker.SelectedDate,
                dt => dt == default ? (DateTimeOffset?)null : new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt)),
                dto => dto?.LocalDateTime ?? default)
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                vm => vm.EstimatedEndDate,
                view => view.EstimatedEndDatePicker.SelectedDate,
                dt => dt == default ? (DateTimeOffset?)null : new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt)),
                dto => dto?.LocalDateTime ?? default)
                .DisposeWith(disposables);

            this.Bind(ViewModel,
                vm => vm.RealEndDate,
                view => view.EndDatePicker.SelectedDate,
                dt => dt == default ? (DateTimeOffset?)null : new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt)),
                dto => dto?.LocalDateTime ?? default)
                .DisposeWith(disposables);

            // Visibility based on permissions
            this.OneWayBind(ViewModel, vm => vm.HasManagerAccess, view => view.InitialGrossAmountParent.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.HasManagerAccess, view => view.ExportOffersButton.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.HasManagerAccess, view => view.ApplicantLabel.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.HasManagerAccess, view => view.ApplicantComboBox.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.HasManagerAccess, view => view.AddOfferButton.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.CanAccept, view => view.AcceptButton.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.CanReject, view => view.RejectButton.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.CanFinish, view => view.FinishButton.IsVisible)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.CanArchive, view => view.ArchiveButton.IsVisible)
                .DisposeWith(disposables);

            // Commands
            this.BindCommand(ViewModel, vm => vm.AddOfferCommand, view => view.AddOfferButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ExportOfferDataToFileCommand, view => view.ExportOffersButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RefreshOffersCommand, view => view.RefreshOffersButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddFileToOfferCommand, view => view.AddOfferFileButton, vm => vm.SelectedOffer)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddInvoicesFileCommand, view => view.AddInvoicesFileButton, vm => vm.SelectedOffer)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SaveOfferCommand, view => view.SaveOfferFileButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CloseDetailsPanelCommand, view => view.CloseDetailsPanelButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AcceptCommand, view => view.AcceptButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RejectCommand, view => view.RejectButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.FinishCommand, view => view.FinishButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ArchiveCommand, view => view.ArchiveButton)
                .DisposeWith(disposables);
        });
    }
}

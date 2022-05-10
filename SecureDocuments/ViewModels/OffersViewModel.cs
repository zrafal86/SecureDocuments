using SecureDocuments.Data;
using SecureDocuments.Helpers;
using SecureDocuments.Models;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using DynamicData;
using DynamicData.Binding;
using Jitbit.Utils;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Splat;
using System.Runtime.Serialization;

namespace SecureDocuments.ViewModels
{
    [DataContract]
    public sealed class OffersViewModel : ViewModelBase, IEnableLogger
    {
        private readonly IOfferService _offersService;
        private readonly IUserService _userService;

        public OffersViewModel(IOfferService? offerService, IUserService? userService, IDialogService? dialogService,
            IScreen? screen = null) :
            base(screen, dialogService)
        {
            _offersService = offerService ?? throw new ArgumentException("Cannot find service. Make sure that it is registered.");
            _userService = userService ?? throw new ArgumentException("Cannot find service. Make sure that it is registered."); ;

            OfferContext = new OfferContext(new DefaultState());
            var statuses = ((IList<Status>)Enum.GetValues(typeof(Status))).Select(status => new StatusLookup(status));
            Statuses = new ObservableCollection<StatusLookup>(statuses);

            var types = ((IList<OfferType>)Enum.GetValues(typeof(OfferType))).Select(type => new OfferTypeLookup(type));
            OfferTypes = new ObservableCollection<OfferTypeLookup>(types);
            CurrencySymbolsCollection = new ObservableCollection<CurrencySymbolLookup>(CurrencyPerCountry.GetCurrencyList());

            AddFileToOfferCommand = ReactiveCommand.CreateFromTask<Offer, IRoutableViewModel>(ShowFilesScreen);
            AddInvoicesFileCommand = ReactiveCommand.CreateFromTask<Offer, IRoutableViewModel>(ShowInvoiceFilesScreen);
            ExportOfferDataToFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CheckPermission(Role.Admin, async () => await ExportOfferDataToFileAction(_offers.Items));
            });
            SaveOfferCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CheckPermission(Role.Manager, async () => await SaveOffer());
            });
            AddOfferCommand = ReactiveCommand.CreateFromObservable<Unit, IRoutableViewModel>(AddOfferAction);
            RefreshOffersCommand = ReactiveCommand.CreateFromTask(RefreshOffersAction);
            CloseDetailsPanelCommand = ReactiveCommand.Create(() => { SelectedOffer = null; }, outputScheduler: RxApp.MainThreadScheduler);

            AcceptCommand = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    if (SelectedApplicant != null &&
                    SelectedManager != null &&
                    SelectedBuilder != null &&
                    SelectedTechnologist != null &&
                    SelectedCompanyName != null &&
                    !string.IsNullOrEmpty(CustomerName))
                    {
                        await CheckPermission(Role.Manager, async () => await ConfirmAction(async () => { await ChangedStatus(Status.ACCEPTED); }));
                    }
                    else
                    {
                        await _dialogService.ShowDialog(
                            "Information",
                            "You need to assign Company name, Customer name, Applicant, Manager, Builder and Technologist into the offer.");
                    }
                },
                this.WhenAnyValue(vm => vm.CanAccept));

            RejectCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SelectedCompanyName != null && !string.IsNullOrEmpty(CustomerName))
                {
                    await CheckPermission(Role.Manager, async () => await ConfirmAction(async () => { await ChangedStatus(Status.REJECTED); }));
                }
                else
                {
                    await _dialogService.ShowDialog(
                        "Information",
                        "You need to assign Company name and Customer name before you reject the offer.");
                }
            },
            this.WhenAnyValue(vm => vm.CanReject));

            FinishCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CheckPermission(Role.Manager, async () => await ConfirmAction(async () => { await ChangedStatus(Status.FINISHED); }));
            },
            this.WhenAnyValue(vm => vm.CanFinish));

            ArchiveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await CheckPermission(Role.Manager, async () => await ConfirmAction(async () => { await ChangedStatus(Status.ARCHIVED); }));
            },
            this.WhenAnyValue(vm => vm.CanArchive));

            HasManagerAccess = _roleAccessService.CheckAccess(Role.Manager);

            _userService.GetUsersAsync(user => user.Role == Role.Admin).ContinueWith(
                t => { Applicants = t.Result?.Select(user => new UserLookup(user)); },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            _userService.GetUsersAsync(user => user.Role == Role.Manager).ContinueWith(
                t => { Managers = t.Result?.Select(user => new UserLookup(user)); },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            _userService.GetUsersAsync(user => user.Role == Role.Builder).ContinueWith(
                t => { Builders = t.Result?.Select(user => new UserLookup(user)); },
                TaskContinuationOptions.OnlyOnRanToCompletion);
            _userService.GetUsersAsync(user => user.Role == Role.Technologist).ContinueWith(
                t => { Technologists = t.Result?.Select(user => new UserLookup(user)); },
                TaskContinuationOptions.OnlyOnRanToCompletion);


            _userService.GetAppConfig().ContinueWith(configurationTask =>
            {
                var config = configurationTask.Result;
                if (config != null)
                {
                    CustomerCountries = config.CustomerCountries?.Select(customerCountry => new CustomerCountriesLookup(customerCountry));
                    UnitFlagCountries = config.CustomerCountries?.Select(customerCountry => new CustomerCountriesLookup(customerCountry));
                    CompanyNames = config.CompanyNames?.Select(cName => new CompanyNameLookup(cName));
                    Subjects = config.Subjects?.Select(cName => new SubjectLookup(cName));
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            _ = MessageBus.Current.Listen<RefreshActionEvent>()
                    .Throttle(TimeSpan.FromMilliseconds(200))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(async x =>
                    {
                        _selectedOfferId = x.Id;
                        if (x.LoadFromFiles)
                        {
                            var offersResult = _offersService.GetOffers();
                            await UpdateOfferList(offersResult);
                        }
                    });
        }

        private Task<Unit> ExportOfferDataToFileAction(IEnumerable<Offer>? offers)
        {
            var myExport = CreateCsvExport(offers);
            var path = PathFactory.GetExportedOffersFile();

            try
            {
                using FileStream fs = File.Create(path);
                foreach (byte i in myExport.ExportToBytes())
                {
                    fs.WriteByte(i);
                }

                var destFolder = PathFactory.GetApplicationLocalFolderPath();
                Process.Start("explorer.exe", destFolder);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }

            return Task.FromResult(Unit.Default);
        }

        private static CsvExport CreateCsvExport(IEnumerable<Offer>? offers)
        {
            var myExport = new CsvExport();

            if (offers != null)
            {
                foreach (var offer in offers)
                {
                    if (offer != null)
                    {
                        myExport.AddRow();
                        myExport["UUID"] = offer.Id;
                        myExport["ID"] = offer.OfferNumber ?? "";

                        myExport["Name"] = offer.Name ?? "";
                        myExport["Description"] = offer.Description ?? "";
                        myExport["Subject"] = !string.IsNullOrEmpty(offer.Subject) ? offer.Subject == "0" ? "" : offer.Subject : "";
                        myExport["Status"] = offer.Status != Status.All ? offer.Status : "";
                        myExport["Type"] = offer.Type;

                        myExport["Company name"] = offer.CompanyName ?? "";
                        myExport["Customer name"] = offer.Customer?.Name ?? "";
                        myExport["Customer address"] = offer.Customer?.Address ?? "";
                        myExport["Initial gross amount"] = offer.InitialGrossAmount ?? "";
                        myExport["ISO currency"] = offer.CurrencySymbol?.ISOCurrencySymbol ?? "";

                        var @default = "0001-01-01";
                        var startDate = offer.StartDate.ToString("yyyy-MM-dd");
                        myExport["Start date"] = startDate.Equals(@default) ? "" : startDate;
                        var acceptedDate = offer.AcceptedDate.ToString("yyyy-MM-dd");
                        myExport["Accepted date"] = acceptedDate.Equals(@default) ? "" : acceptedDate;
                        var createdDate = offer.CreatedTime.ToString("yyyy-MM-dd");
                        myExport["Created date"] = createdDate.Equals(@default) ? "" : createdDate;
                        var realEndDate = offer.RealEndDate.ToString("yyyy-MM-dd");
                        myExport["Real end date"] = realEndDate.Equals(@default) ? "" : realEndDate;
                        var esimatedEndDate = offer.EstimatedEndDate.ToString("yyyy-MM-dd");
                        myExport["Estimated end date"] = esimatedEndDate.Equals(@default) ? "" : esimatedEndDate;

                        myExport["Technologist"] = offer.Technologist?.FullName ?? "";
                        myExport["Technologist email"] = offer.Technologist?.Email ?? "";
                        myExport["Manager"] = offer.Manager?.FullName ?? "";
                        myExport["Manager email"] = offer.Manager?.Email ?? "";
                        myExport["Applicant"] = offer.Applicant?.FullName ?? "";
                        myExport["Applicant email"] = offer.Applicant?.Email ?? "";
                        myExport["Builder"] = offer.Builder?.FullName ?? "";
                        myExport["Builder email"] = offer.Builder?.Email ?? "";
                    }
                }
            }
            return myExport;
        }

        private async Task<Unit> RefreshOffersAction()
        {
            _selectedOfferId = SelectedOffer?.Id ?? _selectedOfferId;
            var offers = _offersService.GetOffers();
            await UpdateOfferList(offers);
            return Unit.Default;
        }

        public override string UrlPathSegment => "Offers";
        private SourceList<Offer> _offers { get; } = new SourceList<Offer>();

        public ReadOnlyObservableCollection<Offer> Offers;
        private string? _selectedOfferId;

        public OfferContext OfferContext { get; init; }
        public ObservableCollection<StatusLookup>? Statuses { get; }
        public ReactiveCommand<Offer, IRoutableViewModel>? AddFileToOfferCommand { get; }
        public ReactiveCommand<Offer, IRoutableViewModel>? AddInvoicesFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportOfferDataToFileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveOfferCommand { get; }
        public ReactiveCommand<Unit, Unit>? CloseDetailsPanelCommand { get; }
        public ReactiveCommand<Unit, IRoutableViewModel>? AddOfferCommand { get; }
        public ReactiveCommand<Unit, Unit>? RefreshOffersCommand { get; }
        public ReactiveCommand<Unit, Unit>? AcceptCommand { get; }
        public ReactiveCommand<Unit, Unit>? RejectCommand { get; }
        public ReactiveCommand<Unit, Unit>? FinishCommand { get; }
        public ReactiveCommand<Unit, Unit>? ArchiveCommand { get; }
        public ObservableCollection<OfferTypeLookup>? OfferTypes { get; }
        public ObservableCollection<CurrencySymbolLookup>? CurrencySymbolsCollection { get; init; }

        [Reactive] public OfferType SelectedOfferType { get; set; }
        [Reactive] public Status SelectedStatus { get; set; }
        [Reactive] public CurrencySymbol SelectedCurrency { get; set; }
        [Reactive] public string SearchTerm { get; set; } = "";
        [DataMember]
        [Reactive] public Offer? SelectedOffer { get; set; }
        [Reactive] public bool HasManagerAccess { get; set; }

        [Reactive] public IEnumerable<UserLookup>? Applicants { get; private set; }
        [Reactive] public IEnumerable<UserLookup>? Managers { get; private set; }
        [Reactive] public IEnumerable<UserLookup>? Builders { get; private set; }
        [Reactive] public IEnumerable<UserLookup>? Technologists { get; private set; }
        [Reactive] public IEnumerable<CompanyNameLookup>? CompanyNames { get; private set; }
        [Reactive] public IEnumerable<SubjectLookup>? Subjects { get; private set; }
        [Reactive] public IEnumerable<CustomerCountriesLookup>? CustomerCountries { get; private set; }
        [Reactive] public IEnumerable<CustomerCountriesLookup>? UnitFlagCountries { get; private set; }
        [Reactive] public User? SelectedApplicant { get; set; }
        [Reactive] public User? SelectedManager { get; set; }
        [Reactive] public bool IsAcceptedRejected { get; set; }
        [Reactive] public User? SelectedBuilder { get; set; }
        [Reactive] public User? SelectedTechnologist { get; set; }
        [Reactive] public CompanyName? SelectedCompanyName { get; set; }
        [Reactive] public Subject? SelectedSubject { get; set; }

        [Reactive] public string? OfferName { get; set; } = "";
        [Reactive] public string? CompanyName { get; set; } = "";
        [Reactive] public OfferSubject Subject { get; set; }
        [Reactive] public string? OfferDescription { get; set; } = "";
        [Reactive] public string? CustomerName { get; set; } = "";
        [Reactive] public string? CustomerAddress { get; set; } = "";
        [Reactive] public string? CustomerDescription { get; set; } = "";
        [Reactive] public CustomerCountry SelectedCustomerCountry { get; set; }
        [Reactive] public CustomerCountry SelectedUnitFlagCountry { get; set; }
        [Reactive] public string? InitialGrossAmount { get; set; } = "";
        [Reactive] public DateTime StartDate { get; set; }
        [Reactive] public DateTime EstimatedEndDate { get; set; }
        [Reactive] public DateTime RealEndDate { get; set; }
        [Reactive] public DateTime AcceptedDate { get; set; }
        [Reactive] public DateTime CreatedTime { get; set; }
        [Reactive] public string OfferId { get; set; }
        [Reactive] public string OfferNumber { get; set; }
        [Reactive] public Status OfferStatus { get; set; }
        public Offer? CurrentOffer { get; private set; }
        public IDialogService? DialogService { get; }
        [Reactive] public bool CanAccept { get; private set; }
        [Reactive] public bool CanReject { get; private set; }
        [Reactive] public bool CanFinish { get; private set; }
        [Reactive] public bool CanArchive { get; private set; }
        [Reactive] public bool CanManagedFiles { get; private set; }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --Offers-- screen!");

            this.WhenAnyValue(vm => vm.SelectedStatus)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async status =>
                {
                    await SetOffersWith(status);
                })
                .DisposeWith(disposables);

            this.WhenAnyValue(vm => vm.SearchTerm)
                .Throttle(TimeSpan.FromSeconds(2))
                .Skip(1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async searchTerm =>
                {
                    var result = SearchOffersWith(searchTerm);
                    await UpdateOfferList(result);
                })
                .DisposeWith(disposables);

            _ = this.WhenAnyValue(viewModel => viewModel.SelectedOffer)
                .Where(offer => offer != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(offer =>
                {
                    SetSelectedOffer(offer!);
                })
                .DisposeWith(disposables);

            _ = _offers.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<Offer>.Descending(o => o.CreatedTime.ToUnixTimeMilliseconds()))
                .Bind(out Offers)
                .Subscribe(offer =>
                {
                    if (_selectedOfferId != null)
                    {
                        SelectOfferBy(Offers, _selectedOfferId);
                    }
                }).DisposeWith(disposables);
        }

        private async Task ChangedStatus(Status status)
        {
            var oldStatus = OfferStatus;
            var newStatus = status;
            var state = OfferContext.ChangedTo(newStatus);
            CanAccept = state.CanAccept;
            CanReject = state.CanReject;
            CanFinish = state.CanFinish;
            CanArchive = state.CanArchive;
            OfferStatus = state.Status;
            CanManagedFiles = state.CanManagedFiles;
            if (status == Status.ACCEPTED)
            {
                AcceptedDate = DateTimeOffset.UtcNow.Date;
            }
#if !DEBUG
            if (_userSettings?.EnableNotification ?? false)
            {
                Services.Notification emalNotify = Locator.Current.GetService<Services.Notification>()!;
                await emalNotify.SendNotification(SelectedOffer, oldStatus, newStatus);
            }
#endif
        }

        private void SetSelectedOffer(Offer offer)
        {
            this.Log().Debug($">>>> offer: {offer.CreatedTime}");
            CurrentOffer = offer;
            SelectedOfferType = offer.Type;
            OfferName = offer.Name;
            CompanyName = offer.CompanyName;
            var companyName = CompanyNames?.FirstOrDefault(companyName => offer.CompanyName != null && companyName.Item.Name.Equals(offer.CompanyName));
            SelectedCompanyName = companyName?.Item;
            var subject = Subjects?.FirstOrDefault(subject => offer != null && !string.IsNullOrEmpty(offer.Subject) && subject.Item.Name.Equals(offer.Subject));
            SelectedSubject = subject?.Item;
            OfferDescription = offer.Description ?? "";
            InitialGrossAmount = offer.InitialGrossAmount ?? "";
            if (offer.Customer != null)
            {
                CustomerName = offer.Customer.Name;
                CustomerAddress = offer.Customer.Address;
                var unitFlag = CustomerCountries?.FirstOrDefault(customerCountry =>
                    offer.Customer.UnitFlagId != 0 && customerCountry.Item.Id == offer.Customer.UnitFlagId);
                if (unitFlag != null)
                {
                    SelectedUnitFlagCountry = unitFlag.Item;
                }
                CustomerDescription = offer.Customer.Description;
                var customerCountry = CustomerCountries?.FirstOrDefault(customerCountry =>
                    offer.Customer.CountryId != 0 && customerCountry.Item.Id == offer.Customer.CountryId);
                if (customerCountry != null)
                {
                    SelectedCustomerCountry = customerCountry.Item;
                }
            }

            var applicant = Applicants?.FirstOrDefault(applicant =>
                offer.Applicant != null && applicant.Item.Email != null && applicant.Item.Email.Equals(offer.Applicant.Email));
            SelectedApplicant = applicant?.Item;
            var manager = Managers?.FirstOrDefault(manager =>
                offer.Manager != null && manager.Item.Email != null && manager.Item.Email.Equals(offer.Manager.Email));
            SelectedManager = manager?.Item;
            var builder = Builders?.FirstOrDefault(builder =>
                offer.Builder != null && builder.Item.Email != null && builder.Item.Email.Equals(offer.Builder.Email));
            SelectedBuilder = builder?.Item;
            var technologist = Technologists?.FirstOrDefault(technologist =>
                offer.Technologist != null && technologist.Item.Email != null && technologist.Item.Email.Equals(offer.Technologist.Email));
            SelectedTechnologist = technologist?.Item;
            IsAcceptedRejected = offer.Status == Status.NEW;

            CreatedTime = offer.CreatedTime.ToLocalTime().DateTime;
            StartDate = offer.StartDate.ToLocalTime().DateTime;
            EstimatedEndDate = offer.EstimatedEndDate.ToLocalTime().DateTime;
            RealEndDate = offer.RealEndDate.ToLocalTime().DateTime;
            AcceptedDate = offer.AcceptedDate.ToLocalTime().Date;

            OfferId = offer.Id ?? "";
            OfferNumber = offer.OfferNumber ?? offer.Id ?? "";
            OfferStatus = offer.Status;
            ChangedStatus(OfferStatus).ContinueWith(result =>
            {
                Log.Logger.Information("Status has changed.");
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            SelectedCurrency = GetCurrencySymbol(offer) ?? new CurrencySymbol { ISOCurrencySymbol = "" };
        }

        private CurrencySymbol? GetCurrencySymbol(Offer offer)
        {
            var first = CurrencySymbolsCollection?.FirstOrDefault(c => c.Item.ISOCurrencySymbol == offer.CurrencySymbol?.ISOCurrencySymbol);
            if (first != null)
            {
                return first!.Item;
            }
            else
            {
                return CurrencySymbolsCollection?.FirstOrDefault(c => c.Item.ISOCurrencySymbol == "EUR")?.Item;
            }
        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --Offers-- screen!");
            _offers.Clear();
        }

        private void SelectOfferBy(IEnumerable<Offer> offers, string? offerId)
        {
            if (offerId != null && offers.Any())
            {
                try
                {
                    var offer = offers.Where(offer => offer.Id != null && offer.Id.Equals(offerId, StringComparison.Ordinal)).Single();
                    SelectedOffer = offer;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
            }
        }

        private IObservable<IRoutableViewModel> AddOfferAction(Unit arg)
        {
            return HostScreen.Router.Navigate.Execute(new NewOfferViewModel(
                Locator.Current.GetService<IUserService>(),
                Locator.Current.GetService<IOfferSource>()));
        }

        private async Task<Unit> SaveOffer()
        {
            Log.Debug("Saving offer");
            var identifier = await _dialogService.ShowWaitDialog();
            Offer offer = CreateOffer();

            await DebugOffer(offer);
            _ = await _offersService.SetOffer(offer);
            _dialogService.Close(identifier);
            MessageBus.Current.SendMessage(new RefreshActionEvent { Id = offer.Id, LoadFromFiles = true });
            return await Task.FromResult(Unit.Default);
        }

        private Offer CreateOffer()
        {
            return new Offer
            {
                Id = OfferId,
                OfferNumber = OfferNumber,
                Name = OfferName,
                CompanyName = SelectedCompanyName != null ? SelectedCompanyName.Name : null,
                Subject = SelectedSubject != null ? SelectedSubject.Name : null,
                Type = SelectedOfferType,
                Status = OfferStatus,
                Manager = SelectedManager,
                Applicant = SelectedApplicant,
                Builder = SelectedBuilder,
                Technologist = SelectedTechnologist,
                Customer = new Customer
                {
                    Name = CustomerName,
                    CountryId = SelectedCustomerCountry?.Id,
                    Address = CustomerAddress,
                    UnitFlagId = SelectedUnitFlagCountry?.Id,
                    Description = CustomerDescription
                },
                Description = OfferDescription,
                InitialGrossAmount = InitialGrossAmount,
                CreatedTime = CreatedTime,

                StartDate = StartDate == default
                    ? DateTimeOffset.UtcNow
                    : new DateTimeOffset(StartDate, TimeZoneInfo.Local.GetUtcOffset(StartDate)),
                RealEndDate = RealEndDate == default
                    ? DateTimeOffset.UtcNow
                    : new DateTimeOffset(RealEndDate, TimeZoneInfo.Local.GetUtcOffset(RealEndDate)),
                EstimatedEndDate = EstimatedEndDate == default
                    ? DateTimeOffset.UtcNow
                    : new DateTimeOffset(EstimatedEndDate, TimeZoneInfo.Local.GetUtcOffset(EstimatedEndDate)),
                AcceptedDate = AcceptedDate == default
                    ? new DateTimeOffset(DateTime.MinValue, TimeZoneInfo.Utc.GetUtcOffset(DateTime.MinValue))
                    : new DateTimeOffset(AcceptedDate, TimeZoneInfo.Local.GetUtcOffset(AcceptedDate)),

                CurrencySymbol = SelectedCurrency
            };
        }

        private async Task DebugOffer(Offer offer)
        {
            var offerJson = await Task.Run(() => JsonConvert.SerializeObject(offer, JsonSerializerConfig.GetConfig()));
            Log.Information($"Offer saved by {_userSettings?.User?.FullName} - now offer is \n{offerJson}");
        }

        private async Task<IRoutableViewModel> ShowFilesScreen(Offer offer)
        {
            return await HostScreen.Router.Navigate.Execute(new OfferFilesViewModel(
                offer,
                FolderPurpose.Normal,
                Locator.Current.GetService<IDialogService>()));
        }

        private async Task<IRoutableViewModel> ShowInvoiceFilesScreen(Offer offer)
        {
            return await HostScreen.Router.Navigate.Execute(new OfferFilesViewModel(
                offer,
                FolderPurpose.Invoices,
                Locator.Current.GetService<IDialogService>()));
        }

        private IAsyncEnumerable<Offer>? SearchOffersWith(string searchTerm)
        {
            this.Log().Info($"search term: {searchTerm}");
            var offers = _offersService.GetOffers();
            if (SelectedStatus != Status.All)
            {
                offers = offers.Where(offer => offer.Status == SelectedStatus);
            }
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchTerms = searchTerm.ToLower().Split(' ');
                offers = SearchForTerms(offers, searchTerms);
            }
            return offers;
        }

        private async Task UpdateOfferList(IAsyncEnumerable<Offer>? offers, bool clean = true)
        {
            if (offers != null)
            {
                if (clean)
                {
                    _offers.Clear();
                }
                await foreach (var offer in offers)
                {
                    if (offer != null)
                    {
                        _offers.Edit(editable =>
                        {
                            var offerOrginal = editable.FirstOrDefault(offerX => offerX.Id.Equals(offer.Id));
                            if (offerOrginal != null)
                            {
                                editable.Replace(offerOrginal, offer);
                            }
                            else
                            {
                                editable.Add(offer);
                            }
                        });
                    }
                }
            }
        }

        private static IAsyncEnumerable<Offer>? SearchForTerms(IAsyncEnumerable<Offer>? offers, string[] searchTerms)
        {
            return searchTerms.Aggregate(
                offers, (current, term) => current?.Where(offer =>
                    offer.Name != null && offer.Name.ToLower().Contains(term) ||
                    offer.Manager != null && offer.Manager.Email != null && offer.Manager.Email.ToLower().Contains(term) ||
                    offer.Manager != null && offer.Manager.FirstName != null && offer.Manager.FirstName.ToLower().Contains(term) ||
                    offer.Manager != null && offer.Manager.LastName != null && offer.Manager.LastName.ToLower().Contains(term) ||
                    offer.Type.ToString().ToLower().Contains(term) ||
                    offer.Customer != null && offer.Customer.Description != null && offer.Customer.Description.ToLower().Contains(term) ||
                    offer.Customer != null && offer.Customer.Name != null && offer.Customer.Name.ToLower().Contains(term)));
        }

        private async Task SetOffersWith(Status status)
        {
            SearchTerm = "";
            var offers = _offersService.GetOffers();
            if (SelectedStatus != Status.All)
            {
                offers = offers
                    .Where(offer => offer.Status == status);
            }
            await UpdateOfferList(offers);
        }

        private async Task<Unit> ConfirmAction(Action methodAction)
        {
            var result = await _dialogService.ShowDialog("Confirmation", "Are you sure to change status?", true);
            if (result != null && result is bool success && success)
            {
                methodAction();
                await SaveOfferCommand.Execute(Unit.Default);
            }

            return await Task.FromResult(Unit.Default);
        }
    }
}
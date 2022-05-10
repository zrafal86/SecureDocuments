#nullable enable

using SecureDocuments.Data;
using SecureDocuments.Models;
using SecureDocuments.Models.Events;
using SecureDocuments.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace SecureDocuments.ViewModels
{
    public sealed class NewOfferViewModel : ViewModelBase
    {
        private readonly IOfferSource _offerSource;
        private readonly IUserService _userService;

        public NewOfferViewModel(
            IUserService userService,
            IOfferSource offerSource,
            IScreen? screen = null) : base(screen)
        {
            _userService = userService;
            _offerSource = offerSource;

            CancelCommand = ReactiveCommand.Create(CancelAction);

            var canAddOffer = this.WhenAnyValue(vm => vm.OfferName, vm => vm.SelectedManager,
                (offerName, selectedManager) =>
                    !string.IsNullOrWhiteSpace(offerName) && selectedManager != null);
            AddCommand = ReactiveCommand.CreateFromTask<Unit, bool>(AddAction, canAddOffer);


            var types = ((IList<OfferType>)Enum.GetValues(typeof(OfferType)))
                .Select(type => new OfferTypeLookup(type));
            OfferTypes = new ObservableCollection<OfferTypeLookup>(types);

            _ = _userService.GetUsersAsync(user => user.Role == Role.Manager).ContinueWith(
                t => { Managers = t.Result.Select(user => new UserLookup(user)); },
                TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(t =>
            {
                var user = Managers?.FirstOrDefault(user => user.Item.FullName.Equals(_userSettings?.User?.FullName));
                if (user != null && user.Item != null) SelectedManager = user.Item;
            });
        }

        public override string UrlPathSegment => "Add new offer";

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }
        public ReactiveCommand<Unit, bool> AddCommand { get; }
        public ObservableCollection<OfferTypeLookup> OfferTypes { get; }

        [Reactive] public string? OfferName { get; set; }
        [Reactive] public string? OfferDescription { get; set; }
        [Reactive] public OfferType SelectedOfferType { get; set; }
        [Reactive] public User SelectedManager { get; set; }
        [Reactive] public string? CustomerName { get; set; }
        [Reactive] public string? CustomerDescription { get; set; }
        [Reactive] public IEnumerable<UserLookup> Managers { get; private set; }

        protected override void HandleActivation(CompositeDisposable disposables)
        {
            Log.Information("Activate --NewOffer-- screen!");
            disposables.Add(this.WhenAnyValue(vm => vm.SelectedOfferType)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(SetOffersWith));
            disposables.Add(this.WhenAnyValue(vm => vm.SelectedManager).Where(user => user != null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(user => { Log.Debug($"user: {user.FullName}"); }));
        }

        protected override void HandleDeactivation()
        {
            Log.Information("Deactivate --NewOffer-- screen!");
        }

        private async Task<bool> AddAction(Unit arg)
        {
            try
            {
                var offer = CreateOffer();
                if (offer != null)
                {
                    var pathToNewOffer = PathFactory.GetPathToNewOfferFile(_userSettings?.ApplicationFolder!, offer); //TODO: find one pleace for that
                    await _offerSource.Save(offer, pathToNewOffer);

                    await HostScreen.Router.NavigateBack.Execute();
                    MessageBus.Current.SendMessage(new RefreshActionEvent { Id = offer.Id! });
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                return false;
            }
        }

        private Offer CreateOffer()
        {
            var createTime = DateTimeOffset.UtcNow;
            var id = Guid.NewGuid();
            var customer = new Customer { Name = CustomerName, Description = CustomerDescription };
            var offer = new Offer
            {
                Id = id.ToString("N"),
                Name = OfferName,
                Type = SelectedOfferType,
                Status = Status.NEW,
                Manager = SelectedManager,
                Customer = customer,
                Description = OfferDescription,
                StartDate = DateTimeOffset.UtcNow,
                RealEndDate = DateTimeOffset.UtcNow,
                EstimatedEndDate = DateTimeOffset.UtcNow,
                AcceptedDate = default,
                CreatedTime = createTime
            };
            return offer;
        }

        private static void SetOffersWith(OfferType type)
        {
            Log.Debug($"SetOffersWith: {type}");
        }

        private void CancelAction()
        {
            HostScreen.Router.NavigateBack.Execute();
        }
    }
}
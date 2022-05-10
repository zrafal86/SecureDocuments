using SecureDocuments.Models;
using SecureDocuments.Services;
using ReactiveUI;
using Splat;

namespace SecureDocuments.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IRoutableViewModel, IActivatableViewModel
    {
        protected readonly IRoleAccessService _roleAccessService;
        protected readonly IDialogService _dialogService;

        protected readonly IUserSettings? _userSettings;

        public ViewModelBase(IScreen? screen = null, IDialogService? dialogService = null)
        {
            Activator = new ViewModelActivator();
            HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
            _userSettings = Locator.Current.GetService<IUserSettings>();
            _dialogService = dialogService ?? Locator.Current.GetService<IDialogService>()!;
            _roleAccessService = Locator.Current.GetService<IRoleAccessService>()!;
            this.WhenActivated(disposables =>
            {
                HandleActivation(disposables);

                Disposable
                    .Create(() => HandleDeactivation())
                    .DisposeWith(disposables);
            });
        }

        public ViewModelActivator Activator { get; }
        public abstract string UrlPathSegment { get; }

        public IScreen HostScreen { get; }

        protected abstract void HandleActivation(CompositeDisposable disposables);
        protected abstract void HandleDeactivation();

        protected async Task<T?> CheckPermission<T>(Role role, Func<T> func)
        {
            var result = _roleAccessService.CheckAccess(role);
            if (!result)
            {
                await ShowNoPermissionDialog();
                return default;
            }
            return func();
        }

        protected async Task CheckPermission(Role role, Action func)
        {
            var result = _roleAccessService.CheckAccess(role);
            if (!result)
            {
                await ShowNoPermissionDialog();
            }
            func?.Invoke();
        }

        private async Task ShowNoPermissionDialog()
        {
            await _dialogService.ShowDialog("No permission", "You do not have sufficient permissions for this action?");
        }

    }
}
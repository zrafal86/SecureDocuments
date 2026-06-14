using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.ViewModels;

namespace SecureDocuments.Avalonia.Views;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    public ProfileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.Bind(ViewModel, vm => vm.EnabledNotification, view => view.ToggleNotification.IsChecked)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.UserEmail, view => view.UserEmailLabel.Text)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.UpdatePassword, view => view.UpdatePasswordButton)
                .DisposeWith(disposables);

            if (ViewModel != null)
            {
                CurrentPassword.GetObservable(TextBox.TextProperty)
                    .Select(t => t ?? "")
                    .Subscribe(passwd => ViewModel.Password = passwd)
                    .DisposeWith(disposables);

                NewPassword.GetObservable(TextBox.TextProperty)
                    .Select(t => t ?? "")
                    .Subscribe(passwd => ViewModel.NewPassword = passwd)
                    .DisposeWith(disposables);
            }
        });
    }
}

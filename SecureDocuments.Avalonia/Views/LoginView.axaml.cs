using Avalonia.ReactiveUI;
using ReactiveUI;
using SecureDocuments.Models.Events;
using SecureDocuments.ViewModels;

namespace SecureDocuments.Avalonia.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
    public LoginView()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.BindCommand(ViewModel, vm => vm.LoginCommand, v => v.LoginButton)
                .DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ChooseAppDirCommand, v => v.ChooseAppDirButton)
                .DisposeWith(disposables);

            this.OneWayBind(ViewModel, vm => vm.ApplicationFolder, v => v.ApplicationDirectory.Text)
                .DisposeWith(disposables);

            this.Bind(ViewModel, vm => vm.UserEmail, v => v.UserEmail.Text)
                .DisposeWith(disposables);

            Password.GetObservable(TextBox.TextProperty)
                .Select(t => t ?? "")
                .Subscribe(passwd => { if (ViewModel != null) ViewModel.UserPassword = passwd; })
                .DisposeWith(disposables);

            Observable.FromEventPattern<KeyEventArgs>(
                h => Password.KeyDown += h,
                h => Password.KeyDown -= h)
                .Where(x => x.EventArgs.Key == Key.Enter)
                .Select(_ => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel!.LoginCommand)
                .DisposeWith(disposables);
        });

        MessageBus.Current.Listen<InvalidUserLoginEvent>().Subscribe(_ =>
        {
            Password.Text = "";
        });
    }
}

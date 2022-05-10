using SecureDocuments.Models.Events;
using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();

            _ = this.WhenActivated(disposables =>
              {
                  this.BindCommand(ViewModel, vm => vm.LoginCommand, v => v.LoginButton)
                      .DisposeWith(disposables);
                  this.BindCommand(ViewModel, vm => vm.ChooseAppDirCommand, v => v.ChooseAppDirButton)
                      .DisposeWith(disposables);

                  this.OneWayBind(ViewModel, vm => vm.ApplicationFolder, v => v.ApplicationDirectory.Text)
                      .DisposeWith(disposables);

                  this.Bind(ViewModel, vm => vm.UserEmail, v => v.UserEmail.Text)
                      .DisposeWith(disposables);

                  Password.Events().PasswordChanged.Select(_ => Password.Password)
                      .Subscribe(passwd => ViewModel!.UserPassword = passwd).DisposeWith(disposables);

                  Password
                      .Events().KeyUp
                      .Where(x => x.Key == Key.Enter)
                      .Select(args => Unit.Default)
                      .InvokeCommand(this, x => x.ViewModel!.LoginCommand)
                      .DisposeWith(disposables);
              });
            MessageBus.Current.Listen<InvalidUserLoginEvent>().Subscribe(eventObject =>
            {
                Password.Password = "";
            });
        }
    }
}
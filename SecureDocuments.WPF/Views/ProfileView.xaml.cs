using ReactiveUI;

namespace SecureDocuments.WPF.Views
{
    /// <summary>
    ///     Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView
    {
        public ProfileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.IsDark,
                    view => view.ToggleDarkMode.IsChecked)
                .DisposeWith(disposables);
                this.Bind(ViewModel,
                    viewModel => viewModel.EnabledNotification,
                    view => view.ToggleNotification.IsChecked)
                .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.UserEmail,
                    view => view.UserEmail.Content)
                .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.UpdatePassword,
                    view => view.UpdatePasswordButton)
                .DisposeWith(disposables);

                if (ViewModel != null)
                {
                    _ = CurrentPassword
                        .Events()
                        .PasswordChanged
                        .Select(_ => CurrentPassword.Password)
                        .Subscribe(passwd => ViewModel.Password = passwd)
                    .DisposeWith(disposables);

                    _ = NewPassword
                        .Events()
                        .PasswordChanged
                        .Select(_ => NewPassword.Password)
                        .Subscribe(passwd => ViewModel.NewPassword = passwd)
                    .DisposeWith(disposables);
                }
            });
        }
    }
}
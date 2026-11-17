using System.Windows;
using FitnessClubIS.ViewModels;

namespace FitnessClubIS.Views;

public partial class AuthorizationWindow : Window, IGettingPassword
{
    public AuthorizationWindow()
    {
        InitializeComponent();
        ((AuthorizationViewModel)DataContext).GettingPassword = this;
    }

    public string GetPassword() => PasswordInput.Password;

    private void LogInButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = (AuthorizationViewModel)DataContext;
        if (vm.LogIn())
        {
            var main = new MainWindow();
            main.Closed += (_, _) => { AuthorizationViewModel.LogOut(); PasswordInput.Clear(); vm.RefreshCaptcha(); Show(); };
            Hide();
            main.Show();
        }
        else
        {
            PasswordInput.Clear();
        }
    }
}

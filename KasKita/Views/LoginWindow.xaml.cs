using System.Windows;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => UsernameBox.Focus();
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm) vm.TryLogin(PwdBox.Password);
    }
}

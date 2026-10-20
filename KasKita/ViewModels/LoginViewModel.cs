using CommunityToolkit.Mvvm.ComponentModel;
using KasKita.Services;

namespace KasKita.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string username = "";
    [ObservableProperty] private string error = "";

    public event Action LoginSuccess;

    public void TryLogin(string password)
    {
        var user = AuthService.Login(Username, password);
        if (user == null)
        {
            Error = "Username atau password salah.";
            return;
        }
        Session.CurrentUser = user;
        LogService.Catat("Login");
        LoginSuccess?.Invoke();
    }
}

using System.Windows;
using KasKita.Data;
using KasKita.Services;
using KasKita.ViewModels;
using KasKita.Views;
using QuestPDF.Infrastructure;

namespace KasKita;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        QuestPDF.Settings.License = LicenseType.Community;

        DispatcherUnhandledException += (s, ex) =>
        {
            MessageBox.Show(ex.Exception.Message, "KasKita - Terjadi kesalahan", MessageBoxButton.OK, MessageBoxImage.Warning);
            ex.Handled = true;
        };

        try { DbSeeder.Init(); }
        catch (Exception ex)
        {
            MessageBox.Show("Gagal menyiapkan basis data:\n" + ex.Message, "KasKita", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        TampilkanLogin();
    }

    private void TampilkanLogin()
    {
        var login = new LoginWindow();
        var loginVm = new LoginViewModel();
        login.DataContext = loginVm;

        loginVm.LoginSuccess += () =>
        {
            var main = new MainWindow();
            var mainVm = new MainViewModel();
            main.DataContext = mainVm;
            mainVm.LogoutRequested += () =>
            {
                Session.CurrentUser = null;
                TampilkanLogin();
                main.Close();
            };

            MainWindow = main;
            main.Show();
            login.Close();

            Toast.Show($"Selamat datang, {mainVm.NamaUser}!");
        };

        MainWindow = login;
        login.Show();
    }
}

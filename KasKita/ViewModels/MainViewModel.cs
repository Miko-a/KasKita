using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Services;
using MaterialDesignThemes.Wpf;

namespace KasKita.ViewModels;

public record NavItem(string Key, string Judul, PackIconKind Ikon);

public partial class MainViewModel : ObservableObject
{
    public event Action LogoutRequested;

    public ObservableCollection<NavItem> NavItems { get; } = new();

    [ObservableProperty] private NavItem selectedNav;
    [ObservableProperty] private PageViewModel currentPage;
    [ObservableProperty] private bool menuExpanded = true;
    [ObservableProperty] private bool isDark;

    public string NamaUser => Session.CurrentUser.Anggota?.Nama ?? Session.CurrentUser.Username;
    public string RoleText => Session.CurrentUser.Role.ToString();

    public MainViewModel()
    {
        NavItems.Add(new NavItem("Dashboard", "Dashboard", PackIconKind.ViewDashboard));
        if (!Session.IsAnggota)
            NavItems.Add(new NavItem("Transaksi", "Transaksi Kas", PackIconKind.SwapHorizontal));
        NavItems.Add(new NavItem("Iuran", "Iuran Anggota", PackIconKind.CashMultiple));
        if (!Session.IsAnggota)
        {
            NavItems.Add(new NavItem("Anggaran", "Anggaran Kegiatan", PackIconKind.Calculator));
            NavItems.Add(new NavItem("Laporan", "Laporan", PackIconKind.FileChart));
        }
        if (Session.IsBendahara)
            NavItems.Add(new NavItem("Master", "Data Master & Sistem", PackIconKind.Cog));

        SelectedNav = NavItems[0];
    }

    private static PageViewModel Buat(string key) => key switch
    {
        "Transaksi" => new TransaksiViewModel(),
        "Iuran" => new IuranViewModel(),
        "Anggaran" => new AnggaranViewModel(),
        "Laporan" => new LaporanViewModel(),
        "Master" => new MasterViewModel(),
        _ => new DashboardViewModel()
    };

    partial void OnSelectedNavChanged(NavItem value)
    {
        if (value != null) CurrentPage = Buat(value.Key);
    }

    partial void OnIsDarkChanged(bool value)
    {
        Session.IsDark = value;
        var helper = new PaletteHelper();
        var theme = helper.GetTheme();
        theme.SetBaseTheme(value ? BaseTheme.Dark : BaseTheme.Light);
        helper.SetTheme(theme);
        // bangun ulang halaman aktif agar warna grafik mengikuti tema
        if (SelectedNav != null) CurrentPage = Buat(SelectedNav.Key);
    }

    [RelayCommand] private void ToggleMenu() => MenuExpanded = !MenuExpanded;

    [RelayCommand]
    private void Logout()
    {
        LogService.Catat("Logout");
        LogoutRequested?.Invoke();
    }
}

using KasKita.Views;
using MaterialDesignThemes.Wpf;

namespace KasKita.Services;

public static class Dialogs
{
    public static async Task<bool> Konfirmasi(string pesan)
    {
        var hasil = await DialogHost.Show(new ConfirmDialog(pesan), "Root");
        return hasil as string == "Ya";
    }
}

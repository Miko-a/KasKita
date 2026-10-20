using KasKita.Models;

namespace KasKita.Services;

public static class Session
{
    public static User CurrentUser { get; set; }
    public static bool IsDark { get; set; }
    public static bool IsBendahara => CurrentUser?.Role == Role.Bendahara;
    public static bool IsKetua => CurrentUser?.Role == Role.Ketua;
    public static bool IsAnggota => CurrentUser?.Role == Role.Anggota;
}

using KasKita.Data;
using KasKita.Models;

namespace KasKita.Services;

public static class LogService
{
    public static void Catat(string aksi)
    {
        using var db = new AppDbContext();
        db.Logs.Add(new LogAktivitas { UserId = Session.CurrentUser?.Id, Aksi = aksi, Waktu = DateTime.Now });
        db.SaveChanges();
    }
}

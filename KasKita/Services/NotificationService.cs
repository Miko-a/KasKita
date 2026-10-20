using KasKita.Data;
using KasKita.Models;
using MaterialDesignThemes.Wpf;

namespace KasKita.Services;

public class Notifikasi
{
    public string Pesan { get; set; } = "";
    public PackIconKind Ikon { get; set; } = PackIconKind.BellOutline;
}

/// <summary>FR-11: peringatan tunggakan, anggaran hampir terlampaui, dan antrean persetujuan.</summary>
public static class NotificationService
{
    public static List<Notifikasi> Ambil()
    {
        var hasil = new List<Notifikasi>();
        var user = Session.CurrentUser;
        if (user == null) return hasil;
        using var db = new AppDbContext();

        if (user.Role == Role.Anggota)
        {
            var n = db.Iurans.Count(i => i.AnggotaId == user.AnggotaId && i.Status == StatusIuran.Menunggak);
            if (n > 0)
                hasil.Add(new Notifikasi { Pesan = $"Anda memiliki {n} tagihan iuran yang belum dibayar.", Ikon = PackIconKind.AlertCircleOutline });
            return hasil;
        }

        var tunggak = db.Iurans.Where(i => i.Status == StatusIuran.Menunggak).Select(i => i.Nominal).ToList();
        if (tunggak.Count > 0)
            hasil.Add(new Notifikasi { Pesan = $"{tunggak.Count} tagihan iuran menunggak (total {DataHelper.Rp(tunggak.Sum())}).", Ikon = PackIconKind.AlertCircleOutline });

        if (user.Role == Role.Ketua)
        {
            var tx = db.Transaksis.Count(t => t.Status == StatusPersetujuan.Menunggu);
            if (tx > 0) hasil.Add(new Notifikasi { Pesan = $"{tx} pengeluaran menunggu persetujuan Anda.", Ikon = PackIconKind.CheckDecagramOutline });
            var kg = db.Kegiatans.Count(k => k.Status == StatusPersetujuan.Menunggu);
            if (kg > 0) hasil.Add(new Notifikasi { Pesan = $"{kg} pengajuan anggaran menunggu persetujuan Anda.", Ikon = PackIconKind.CheckDecagramOutline });
        }

        var realisasi = DataHelper.RealisasiPerKegiatan(db);
        foreach (var k in db.Kegiatans.Where(k => k.Status == StatusPersetujuan.Disetujui && k.TotalAnggaran > 0).ToList())
        {
            realisasi.TryGetValue(k.Id, out var r);
            var persen = (double)r / k.TotalAnggaran * 100;
            if (persen >= 90)
                hasil.Add(new Notifikasi
                {
                    Pesan = persen > 100
                        ? $"Anggaran \"{k.Nama}\" sudah terlampaui ({persen:N0}%)."
                        : $"Anggaran \"{k.Nama}\" hampir habis ({persen:N0}%).",
                    Ikon = PackIconKind.AlertOutline
                });
        }
        return hasil;
    }
}

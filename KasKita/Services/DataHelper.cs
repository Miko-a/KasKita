using System.Globalization;
using KasKita.Data;
using KasKita.Models;

namespace KasKita.Services;

public static class DataHelper
{
    public static readonly CultureInfo Id = new("id-ID");

    public static string Rp(long nilai) => "Rp " + nilai.ToString("N0", Id);

    public static Periode PeriodeAktif(AppDbContext db) =>
        db.Periodes.OrderByDescending(p => p.TanggalMulai).First();

    /// <summary>Menyalin berkas bukti ke folder data aplikasi dan mengembalikan path barunya.</summary>
    public static string SimpanBukti(string sumber)
    {
        if (string.IsNullOrWhiteSpace(sumber) || !File.Exists(sumber)) return null;
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KasKita", "Bukti");
        Directory.CreateDirectory(dir);
        var tujuan = Path.Combine(dir, $"{Guid.NewGuid():N}_{Path.GetFileName(sumber)}");
        File.Copy(sumber, tujuan);
        return tujuan;
    }

    /// <summary>Realisasi (pengeluaran disetujui) per kegiatan.</summary>
    public static Dictionary<int, long> RealisasiPerKegiatan(AppDbContext db) =>
        db.Transaksis
            .Where(t => t.KegiatanId != null && t.Jenis == JenisKas.Keluar && t.Status == StatusPersetujuan.Disetujui)
            .AsEnumerable()
            .GroupBy(t => t.KegiatanId.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Nominal));
}

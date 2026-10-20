using System.Globalization;

namespace KasKita.Models;

public enum Role { Bendahara, Ketua, Anggota }
public enum JenisKas { Masuk, Keluar }
public enum StatusPersetujuan { Menunggu, Disetujui, Ditolak }
public enum StatusIuran { Menunggak, Lunas }

public static class Konstanta
{
    /// <summary>Pengeluaran >= nilai ini harus disetujui Ketua.</summary>
    public const long AmbangPersetujuan = 500_000;
    public const long NominalIuranDefault = 50_000;
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Role Role { get; set; }
    public int? AnggotaId { get; set; }
    public Anggota Anggota { get; set; }
}

public class Anggota
{
    public int Id { get; set; }
    public string Nim { get; set; } = "";
    public string Nama { get; set; } = "";
    public string Jabatan { get; set; } = "Anggota";
    public bool Aktif { get; set; } = true;
    public ICollection<Iuran> Iurans { get; set; } = new List<Iuran>();
}

public class Periode
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public DateTime TanggalMulai { get; set; }
    public DateTime TanggalAkhir { get; set; }
}

public class Kategori
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public JenisKas Jenis { get; set; }
}

public class Transaksi
{
    public int Id { get; set; }
    public DateTime Tanggal { get; set; } = DateTime.Today;
    public long Nominal { get; set; }
    public JenisKas Jenis { get; set; }
    public string Keterangan { get; set; } = "";
    public string BuktiPath { get; set; }
    public StatusPersetujuan Status { get; set; } = StatusPersetujuan.Disetujui;
    public string Catatan { get; set; }

    public int KategoriId { get; set; }
    public Kategori Kategori { get; set; }
    public int PeriodeId { get; set; }
    public Periode Periode { get; set; }
    public int? KegiatanId { get; set; }
    public Kegiatan Kegiatan { get; set; }

    public string BuktiNama
    {
        get
        {
            if (string.IsNullOrEmpty(BuktiPath)) return "-";
            var f = Path.GetFileName(BuktiPath);
            return f.Length > 33 ? f.Substring(33) : f; // prefiks GUID (32) + "_"
        }
    }
}

public class Iuran
{
    public int Id { get; set; }
    public int AnggotaId { get; set; }
    public Anggota Anggota { get; set; }
    public int PeriodeId { get; set; }
    public Periode Periode { get; set; }
    public int Bulan { get; set; }
    public int Tahun { get; set; }
    public long Nominal { get; set; }
    public StatusIuran Status { get; set; } = StatusIuran.Menunggak;
    public DateTime? TanggalBayar { get; set; }

    public string BulanNama => new DateTime(Tahun, Bulan, 1).ToString("MMMM yyyy", new CultureInfo("id-ID"));
}

public class Kegiatan
{
    public int Id { get; set; }
    public string Nama { get; set; } = "";
    public DateTime Tanggal { get; set; } = DateTime.Today;
    public long TotalAnggaran { get; set; }
    public StatusPersetujuan Status { get; set; } = StatusPersetujuan.Menunggu;
    public string Catatan { get; set; }
    public int? PeriodeId { get; set; }
    public Periode Periode { get; set; }
    public ICollection<AnggaranItem> Items { get; set; } = new List<AnggaranItem>();
    public ICollection<Transaksi> Transaksis { get; set; } = new List<Transaksi>();
}

public class AnggaranItem
{
    public int Id { get; set; }
    public int KegiatanId { get; set; }
    public Kegiatan Kegiatan { get; set; }
    public string Uraian { get; set; } = "";
    public long Jumlah { get; set; }
}

public class LogAktivitas
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User User { get; set; }
    public string Aksi { get; set; } = "";
    public DateTime Waktu { get; set; } = DateTime.Now;
}

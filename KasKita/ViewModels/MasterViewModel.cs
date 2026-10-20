using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Data;
using KasKita.Models;
using KasKita.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace KasKita.ViewModels;

/// <summary>FR-02 (anggota, periode), FR-04 (kategori), FR-12 (log, backup, restore).</summary>
public partial class MasterViewModel : PageViewModel
{
    public ObservableCollection<Anggota> Anggotas { get; } = new();
    public ObservableCollection<Kategori> Kategoris { get; } = new();
    public ObservableCollection<Periode> Periodes { get; } = new();
    public ObservableCollection<LogAktivitas> Logs { get; } = new();
    public JenisKas[] JenisOptions { get; } = Enum.GetValues<JenisKas>();

    [ObservableProperty] private string nimBaru = "";
    [ObservableProperty] private string namaBaru = "";
    [ObservableProperty] private string jabatanBaru = "Anggota";
    [ObservableProperty] private string kategoriNama = "";
    [ObservableProperty] private JenisKas kategoriJenis = JenisKas.Keluar;
    [ObservableProperty] private string periodeNama = "";
    [ObservableProperty] private DateTime periodeMulai = new(DateTime.Today.Year, 1, 1);
    [ObservableProperty] private DateTime periodeAkhir = new(DateTime.Today.Year, 12, 31);
    [ObservableProperty] private string cariAnggota = "";

    public MasterViewModel() => Muat();

    partial void OnCariAnggotaChanged(string value) => Muat();

    private void Muat()
    {
        using var db = new AppDbContext();
        var q = db.Anggotas.AsQueryable();
        if (!string.IsNullOrWhiteSpace(CariAnggota))
        {
            var s = CariAnggota.Trim().ToLower();
            q = q.Where(a => a.Nama.ToLower().Contains(s) || a.Nim.Contains(s));
        }
        Isi(Anggotas, q.OrderBy(a => a.Nama).ToList());
        Isi(Kategoris, db.Kategoris.OrderBy(k => k.Jenis).ThenBy(k => k.Nama).ToList());
        Isi(Periodes, db.Periodes.OrderByDescending(p => p.TanggalMulai).ToList());
        Isi(Logs, db.Logs.Include(l => l.User).OrderByDescending(l => l.Waktu).Take(100).ToList());
    }

    private static void Isi<T>(ObservableCollection<T> target, IEnumerable<T> data)
    {
        target.Clear();
        foreach (var d in data) target.Add(d);
    }

    // ---------- Anggota ----------
    [RelayCommand]
    private void TambahAnggota()
    {
        if (string.IsNullOrWhiteSpace(NimBaru) || string.IsNullOrWhiteSpace(NamaBaru))
        {
            Toast.Show("NIM/NRP dan nama wajib diisi.");
            return;
        }
        using var db = new AppDbContext();
        if (db.Anggotas.Any(a => a.Nim == NimBaru.Trim()))
        {
            Toast.Show("NIM/NRP sudah terdaftar.");
            return;
        }
        db.Anggotas.Add(new Anggota { Nim = NimBaru.Trim(), Nama = NamaBaru.Trim(), Jabatan = JabatanBaru.Trim() });
        db.SaveChanges();
        LogService.Catat($"Tambah anggota {NamaBaru}");
        NimBaru = ""; NamaBaru = ""; JabatanBaru = "Anggota";
        Muat();
        Toast.Show("Anggota ditambahkan.");
    }

    public void SimpanAnggota(Anggota a)
    {
        if (a == null) return;
        using var db = new AppDbContext();
        var e = db.Anggotas.Find(a.Id);
        if (e == null) return;
        e.Nim = a.Nim; e.Nama = a.Nama; e.Jabatan = a.Jabatan;
        db.SaveChanges();
        LogService.Catat($"Ubah anggota #{a.Id}");
        Toast.Show("Data anggota disimpan.");
    }

    [RelayCommand]
    private void ToggleAktif(Anggota a)
    {
        if (a == null) return;
        using var db = new AppDbContext();
        var e = db.Anggotas.Find(a.Id);
        if (e == null) return;
        e.Aktif = !e.Aktif;
        db.SaveChanges();
        LogService.Catat($"{(e.Aktif ? "Aktifkan" : "Nonaktifkan")} anggota {e.Nama}");
        Muat();
    }

    [RelayCommand]
    private async Task HapusAnggota(Anggota a)
    {
        if (a == null) return;
        using (var cek = new AppDbContext())
        {
            if (cek.Iurans.Any(i => i.AnggotaId == a.Id) || cek.Users.Any(u => u.AnggotaId == a.Id))
            {
                Toast.Show("Anggota punya data iuran/akun. Nonaktifkan saja.");
                return;
            }
        }
        if (!await Dialogs.Konfirmasi($"Hapus anggota {a.Nama}?")) return;
        using var db = new AppDbContext();
        var e = db.Anggotas.Find(a.Id);
        if (e != null) { db.Anggotas.Remove(e); db.SaveChanges(); }
        LogService.Catat($"Hapus anggota {a.Nama}");
        Muat();
    }

    // ---------- Kategori ----------
    [RelayCommand]
    private void TambahKategori()
    {
        if (string.IsNullOrWhiteSpace(KategoriNama)) { Toast.Show("Nama kategori wajib diisi."); return; }
        using var db = new AppDbContext();
        db.Kategoris.Add(new Kategori { Nama = KategoriNama.Trim(), Jenis = KategoriJenis });
        db.SaveChanges();
        LogService.Catat($"Tambah kategori {KategoriNama}");
        KategoriNama = "";
        Muat();
        Toast.Show("Kategori ditambahkan.");
    }

    [RelayCommand]
    private async Task HapusKategori(Kategori k)
    {
        if (k == null) return;
        using (var cek = new AppDbContext())
        {
            if (cek.Transaksis.Any(t => t.KategoriId == k.Id))
            {
                Toast.Show("Kategori sudah dipakai transaksi, tidak dapat dihapus.");
                return;
            }
        }
        if (!await Dialogs.Konfirmasi($"Hapus kategori {k.Nama}?")) return;
        using var db = new AppDbContext();
        var e = db.Kategoris.Find(k.Id);
        if (e != null) { db.Kategoris.Remove(e); db.SaveChanges(); }
        LogService.Catat($"Hapus kategori {k.Nama}");
        Muat();
    }

    // ---------- Periode ----------
    [RelayCommand]
    private void TambahPeriode()
    {
        if (string.IsNullOrWhiteSpace(PeriodeNama) || PeriodeAkhir <= PeriodeMulai)
        {
            Toast.Show("Nama wajib diisi dan tanggal akhir harus setelah tanggal mulai.");
            return;
        }
        using var db = new AppDbContext();
        db.Periodes.Add(new Periode { Nama = PeriodeNama.Trim(), TanggalMulai = PeriodeMulai.Date, TanggalAkhir = PeriodeAkhir.Date });
        db.SaveChanges();
        LogService.Catat($"Tambah periode {PeriodeNama}");
        PeriodeNama = "";
        Muat();
        Toast.Show("Periode ditambahkan (periode terbaru otomatis menjadi periode aktif).");
    }

    // ---------- Backup / Restore ----------
    [RelayCommand]
    private void Backup()
    {
        var dlg = new SaveFileDialog { Filter = "Basis data KasKita|*.db", FileName = $"kaskita_backup_{DateTime.Now:yyyyMMdd_HHmm}.db" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            BackupService.Backup(dlg.FileName);
            LogService.Catat("Backup basis data");
            Toast.Show("Backup berhasil.");
        }
        catch (Exception ex) { Toast.Show("Backup gagal: " + ex.Message); }
    }

    [RelayCommand]
    private async Task Restore()
    {
        var dlg = new OpenFileDialog { Filter = "Basis data KasKita|*.db" };
        if (dlg.ShowDialog() != true) return;
        if (!await Dialogs.Konfirmasi("Restore akan menimpa seluruh data saat ini. Lanjutkan?")) return;
        try
        {
            BackupService.Restore(dlg.FileName);
            Toast.Show("Restore berhasil. Tutup lalu buka kembali aplikasi.");
        }
        catch (Exception ex) { Toast.Show("Restore gagal: " + ex.Message); }
    }
}

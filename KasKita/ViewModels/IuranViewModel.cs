using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Data;
using KasKita.Models;
using KasKita.Services;
using Microsoft.EntityFrameworkCore;

namespace KasKita.ViewModels;

public record BulanItem(int Angka, string Nama);

public partial class IuranViewModel : PageViewModel
{
    private bool _siap;

    [ObservableProperty] private ObservableCollection<Iuran> items = new();
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private string filterStatus = "Semua";
    [ObservableProperty] private bool hanyaPenunggak;
    [ObservableProperty] private int selectedBulan = DateTime.Today.Month;
    [ObservableProperty] private int selectedTahun = DateTime.Today.Year;
    [ObservableProperty] private string nominalIuran = Konstanta.NominalIuranDefault.ToString();
    [ObservableProperty] private int jumlahTunggakan;
    [ObservableProperty] private long totalTunggakan;

    public List<BulanItem> Bulans { get; } = Enumerable.Range(1, 12)
        .Select(i => new BulanItem(i, new DateTime(2000, i, 1).ToString("MMMM", DataHelper.Id))).ToList();
    public List<int> Tahuns { get; } = Enumerable.Range(DateTime.Today.Year - 2, 4).ToList();
    public string[] StatusOptions { get; } = { "Semua", "Menunggak", "Lunas" };

    public IuranViewModel()
    {
        _siap = true;
        Load();
    }

    partial void OnSearchTextChanged(string value) => Muat();
    partial void OnFilterStatusChanged(string value) => Muat();
    partial void OnHanyaPenunggakChanged(bool value) => Muat();
    partial void OnSelectedBulanChanged(int value) => Muat();
    partial void OnSelectedTahunChanged(int value) => Muat();

    private void Muat() { if (_siap) Load(); }

    private void Load()
    {
        using var db = new AppDbContext();
        IQueryable<Iuran> q = db.Iurans.Include(i => i.Anggota);

        if (Session.IsAnggota)
        {
            // Anggota: hanya melihat iuran pribadinya
            var id = Session.CurrentUser.AnggotaId;
            q = q.Where(i => i.AnggotaId == id);
        }
        else if (HanyaPenunggak)
        {
            q = q.Where(i => i.Status == StatusIuran.Menunggak);
        }
        else
        {
            var b = SelectedBulan; var t = SelectedTahun;
            q = q.Where(i => i.Bulan == b && i.Tahun == t);
        }

        if (!HanyaPenunggak && FilterStatus != "Semua" && Enum.TryParse<StatusIuran>(FilterStatus, out var st))
            q = q.Where(i => i.Status == st);
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLower();
            q = q.Where(i => i.Anggota.Nama.ToLower().Contains(s) || i.Anggota.Nim.Contains(s));
        }

        var data = q.OrderBy(i => i.Anggota.Nama).ThenBy(i => i.Tahun).ThenBy(i => i.Bulan).ToList();
        Items = new ObservableCollection<Iuran>(data);

        var tunggak = data.Where(i => i.Status == StatusIuran.Menunggak).ToList();
        JumlahTunggakan = tunggak.Count;
        TotalTunggakan = tunggak.Sum(i => i.Nominal);
    }

    [RelayCommand]
    private void BuatTagihan()
    {
        if (!IsBendahara) return;
        var teks = (NominalIuran ?? "").Replace(".", "").Replace(",", "").Trim();
        if (!long.TryParse(teks, out var nominal) || nominal <= 0)
        {
            Toast.Show("Nominal iuran tidak valid.");
            return;
        }

        using var db = new AppDbContext();
        var periode = DataHelper.PeriodeAktif(db);
        var sudah = db.Iurans.Where(i => i.Bulan == SelectedBulan && i.Tahun == SelectedTahun)
            .Select(i => i.AnggotaId).ToHashSet();
        int dibuat = 0;
        foreach (var a in db.Anggotas.Where(a => a.Aktif).ToList())
        {
            if (sudah.Contains(a.Id)) continue;
            db.Iurans.Add(new Iuran
            {
                AnggotaId = a.Id, PeriodeId = periode.Id, Bulan = SelectedBulan, Tahun = SelectedTahun,
                Nominal = nominal, Status = StatusIuran.Menunggak
            });
            dibuat++;
        }
        db.SaveChanges();
        LogService.Catat($"Buat tagihan iuran {SelectedBulan}/{SelectedTahun} ({dibuat} anggota)");
        Load();
        Toast.Show(dibuat > 0 ? $"{dibuat} tagihan iuran dibuat." : "Semua anggota aktif sudah memiliki tagihan bulan ini.");
    }

    [RelayCommand]
    private void Bayar(Iuran iuran)
    {
        if (iuran == null || !IsBendahara || iuran.Status == StatusIuran.Lunas) return;
        using var db = new AppDbContext();
        var e = db.Iurans.Include(i => i.Anggota).FirstOrDefault(i => i.Id == iuran.Id);
        if (e == null) return;

        e.Status = StatusIuran.Lunas;
        e.TanggalBayar = DateTime.Now;

        // pembayaran otomatis tercatat sebagai kas masuk
        var kategori = db.Kategoris.FirstOrDefault(k => k.Nama == "Iuran Anggota");
        if (kategori == null)
        {
            kategori = new Kategori { Nama = "Iuran Anggota", Jenis = JenisKas.Masuk };
            db.Kategoris.Add(kategori);
        }
        db.Transaksis.Add(new Transaksi
        {
            Tanggal = DateTime.Today, Nominal = e.Nominal, Jenis = JenisKas.Masuk, Kategori = kategori,
            PeriodeId = e.PeriodeId, Status = StatusPersetujuan.Disetujui,
            Keterangan = $"Iuran {e.Anggota.Nama} - {e.BulanNama}"
        });
        db.SaveChanges();
        LogService.Catat($"Pembayaran iuran {e.Anggota.Nama} {e.BulanNama}");
        Load();
        Toast.Show($"Iuran {e.Anggota.Nama} ditandai lunas.");
    }
}

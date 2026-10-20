using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Data;
using KasKita.Models;
using KasKita.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace KasKita.ViewModels;

public partial class TransaksiViewModel : PageViewModel
{
    private const int UkuranHalaman = 12;
    private bool _siap;
    private bool _memuat;
    private List<Kategori> _semuaKategori = new();

    [ObservableProperty] private ObservableCollection<Transaksi> items = new();
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private Kategori filterKategori;
    [ObservableProperty] private string filterStatus = "Semua";
    [ObservableProperty] private DateTime? dateFrom;
    [ObservableProperty] private DateTime? dateTo;
    [ObservableProperty] private int page = 1;
    [ObservableProperty] private int totalPages = 1;
    [ObservableProperty] private int totalCount;

    // form tambah
    [ObservableProperty] private DateTime newTanggal = DateTime.Today;
    [ObservableProperty] private JenisKas newJenis = JenisKas.Keluar;
    [ObservableProperty] private Kategori newKategori;
    [ObservableProperty] private string newNominal = "";
    [ObservableProperty] private string newKeterangan = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(NewBuktiNama))] private string newBuktiPath = "";
    [ObservableProperty] private Kegiatan newKegiatan;
    [ObservableProperty] private string catatanPersetujuan = "";

    public string NewBuktiNama => string.IsNullOrEmpty(NewBuktiPath)
        ? "Seret bukti ke sini / klik untuk memilih berkas"
        : Path.GetFileName(NewBuktiPath);

    public ObservableCollection<Kategori> KategoriFilterList { get; } = new();
    public ObservableCollection<Kategori> KategoriForm { get; } = new();
    public ObservableCollection<Kegiatan> KegiatanList { get; } = new();
    public string[] StatusOptions { get; } = { "Semua", "Menunggu", "Disetujui", "Ditolak" };
    public JenisKas[] JenisOptions { get; } = Enum.GetValues<JenisKas>();

    public TransaksiViewModel()
    {
        using (var db = new AppDbContext())
        {
            _semuaKategori = db.Kategoris.OrderBy(k => k.Nama).ToList();
            KategoriFilterList.Add(new Kategori { Id = 0, Nama = "Semua kategori" });
            foreach (var k in _semuaKategori) KategoriFilterList.Add(k);

            KegiatanList.Add(new Kegiatan { Id = 0, Nama = "(Tanpa kegiatan)" });
            foreach (var k in db.Kegiatans.Where(k => k.Status == StatusPersetujuan.Disetujui).OrderBy(k => k.Nama))
                KegiatanList.Add(k);
        }
        FilterKategori = KategoriFilterList[0];
        NewKegiatan = KegiatanList[0];
        RefreshKategoriForm();
        _siap = true;
        Load();
    }

    private void RefreshKategoriForm()
    {
        KategoriForm.Clear();
        foreach (var k in _semuaKategori.Where(k => k.Jenis == NewJenis)) KategoriForm.Add(k);
        NewKategori = KategoriForm.FirstOrDefault();
    }

    partial void OnNewJenisChanged(JenisKas value) => RefreshKategoriForm();
    partial void OnSearchTextChanged(string value) => Reload();
    partial void OnFilterKategoriChanged(Kategori value) => Reload();
    partial void OnFilterStatusChanged(string value) => Reload();
    partial void OnDateFromChanged(DateTime? value) => Reload();
    partial void OnDateToChanged(DateTime? value) => Reload();
    partial void OnPageChanged(int value) { if (_siap && !_memuat) Load(); }

    private void Reload()
    {
        if (!_siap) return;
        if (Page != 1) Page = 1; else Load();
    }

    private void Load()
    {
        _memuat = true;
        try
        {
            using var db = new AppDbContext();
            IQueryable<Transaksi> q = db.Transaksis.Include(t => t.Kategori).Include(t => t.Kegiatan);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.Trim().ToLower();
                q = q.Where(t => t.Keterangan.ToLower().Contains(s) || t.Kategori.Nama.ToLower().Contains(s));
            }
            if (FilterKategori != null && FilterKategori.Id != 0)
            {
                var id = FilterKategori.Id;
                q = q.Where(t => t.KategoriId == id);
            }
            if (FilterStatus != "Semua" && Enum.TryParse<StatusPersetujuan>(FilterStatus, out var st))
                q = q.Where(t => t.Status == st);
            if (DateFrom.HasValue)
            {
                var d = DateFrom.Value.Date;
                q = q.Where(t => t.Tanggal >= d);
            }
            if (DateTo.HasValue)
            {
                var d = DateTo.Value.Date.AddDays(1);
                q = q.Where(t => t.Tanggal < d);
            }

            TotalCount = q.Count();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)UkuranHalaman));
            if (Page > TotalPages) Page = TotalPages;
            if (Page < 1) Page = 1;

            var data = q.OrderByDescending(t => t.Tanggal).ThenByDescending(t => t.Id)
                .Skip((Page - 1) * UkuranHalaman).Take(UkuranHalaman).ToList();
            Items = new ObservableCollection<Transaksi>(data);
        }
        finally { _memuat = false; }
    }

    [RelayCommand]
    private void Tambah()
    {
        if (!IsBendahara) return;
        var teks = (NewNominal ?? "").Replace(".", "").Replace(",", "").Trim();
        if (!long.TryParse(teks, out var nominal) || nominal <= 0)
        {
            Toast.Show("Nominal tidak valid.");
            return;
        }
        if (NewKategori == null) { Toast.Show("Pilih kategori terlebih dahulu."); return; }
        if (string.IsNullOrWhiteSpace(NewKeterangan)) { Toast.Show("Keterangan wajib diisi."); return; }

        using var db = new AppDbContext();
        var periode = DataHelper.PeriodeAktif(db);
        bool perluSetuju = NewJenis == JenisKas.Keluar && nominal >= Konstanta.AmbangPersetujuan;

        db.Transaksis.Add(new Transaksi
        {
            Tanggal = NewTanggal.Date,
            Nominal = nominal,
            Jenis = NewJenis,
            KategoriId = NewKategori.Id,
            PeriodeId = periode.Id,
            KegiatanId = NewKegiatan is { Id: > 0 } ? NewKegiatan.Id : null,
            Keterangan = NewKeterangan.Trim(),
            BuktiPath = DataHelper.SimpanBukti(NewBuktiPath),
            Status = perluSetuju ? StatusPersetujuan.Menunggu : StatusPersetujuan.Disetujui
        });
        db.SaveChanges();
        LogService.Catat($"Tambah transaksi {NewJenis} {DataHelper.Rp(nominal)}: {NewKeterangan}");

        NewNominal = "";
        NewKeterangan = "";
        NewBuktiPath = "";
        Load();
        Toast.Show(perluSetuju ? "Transaksi disimpan, menunggu persetujuan Ketua." : "Transaksi berhasil disimpan.");
    }

    public override void Simpan() => Tambah();

    [RelayCommand]
    private async Task Hapus(Transaksi t)
    {
        if (t == null || !IsBendahara) return;
        if (!await Dialogs.Konfirmasi($"Hapus transaksi \"{t.Keterangan}\"?")) return;
        using var db = new AppDbContext();
        var e = db.Transaksis.Find(t.Id);
        if (e != null)
        {
            db.Transaksis.Remove(e);
            db.SaveChanges();
            LogService.Catat($"Hapus transaksi #{t.Id}: {t.Keterangan}");
        }
        Load();
        Toast.Show("Transaksi dihapus.");
    }

    [RelayCommand] private void Setujui(Transaksi t) => Putuskan(t, StatusPersetujuan.Disetujui);
    [RelayCommand] private void Tolak(Transaksi t) => Putuskan(t, StatusPersetujuan.Ditolak);

    private void Putuskan(Transaksi t, StatusPersetujuan status)
    {
        if (t == null || !IsKetua) return;
        using var db = new AppDbContext();
        var e = db.Transaksis.Find(t.Id);
        if (e == null) return;
        e.Status = status;
        e.Catatan = CatatanPersetujuan;
        db.SaveChanges();
        LogService.Catat($"{status} transaksi #{t.Id}");
        CatatanPersetujuan = "";
        Load();
        Toast.Show(status == StatusPersetujuan.Disetujui ? "Pengeluaran disetujui." : "Pengeluaran ditolak.");
    }

    /// <summary>Dipanggil dari code-behind setelah inline editing selesai (UI-02).</summary>
    public void SimpanEdit(Transaksi t)
    {
        if (t == null || !IsBendahara) return;
        using var db = new AppDbContext();
        var e = db.Transaksis.Find(t.Id);
        if (e == null) return;
        e.Keterangan = t.Keterangan;
        e.Nominal = t.Nominal;
        db.SaveChanges();
        LogService.Catat($"Ubah transaksi #{t.Id}");
        Toast.Show("Perubahan disimpan.");
    }

    [RelayCommand] private void Berikutnya() { if (Page < TotalPages) Page++; }
    [RelayCommand] private void Sebelumnya() { if (Page > 1) Page--; }

    [RelayCommand]
    private void Reset()
    {
        _siap = false;
        SearchText = "";
        FilterKategori = KategoriFilterList[0];
        FilterStatus = "Semua";
        DateFrom = null;
        DateTo = null;
        _siap = true;
        Reload();
    }

    [RelayCommand]
    private void PilihBukti()
    {
        var dlg = new OpenFileDialog { Filter = "Gambar/PDF|*.png;*.jpg;*.jpeg;*.pdf|Semua berkas|*.*" };
        if (dlg.ShowDialog() == true) NewBuktiPath = dlg.FileName;
    }

    [RelayCommand]
    private void BukaBukti(Transaksi t)
    {
        if (t != null && !string.IsNullOrEmpty(t.BuktiPath) && File.Exists(t.BuktiPath))
            Process.Start(new ProcessStartInfo(t.BuktiPath) { UseShellExecute = true });
    }
}

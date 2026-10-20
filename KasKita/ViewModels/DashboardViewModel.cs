using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KasKita.Data;
using KasKita.Models;
using KasKita.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace KasKita.ViewModels;

public partial class DashboardViewModel : PageViewModel
{
    public string[] FilterOptions { get; } = { "3 bulan terakhir", "6 bulan terakhir", "12 bulan terakhir" };

    [ObservableProperty] private string selectedFilter = "6 bulan terakhir";
    [ObservableProperty] private long saldo;
    [ObservableProperty] private long totalMasuk;
    [ObservableProperty] private long totalKeluar;
    [ObservableProperty] private int jumlahTunggakan;
    [ObservableProperty] private long totalTunggakan;

    [ObservableProperty] private ISeries[] trenSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] trenX = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] trenY = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] barSeries = Array.Empty<ISeries>();
    [ObservableProperty] private Axis[] barX = Array.Empty<Axis>();
    [ObservableProperty] private Axis[] barY = Array.Empty<Axis>();
    [ObservableProperty] private ISeries[] komposisiSeries = Array.Empty<ISeries>();
    [ObservableProperty] private SolidColorPaint legendPaint;

    public ObservableCollection<Notifikasi> Notifikasis { get; } = new();

    public DashboardViewModel() => Load();

    partial void OnSelectedFilterChanged(string value) => Load();

    private void Load()
    {
        int n = SelectedFilter.StartsWith("3") ? 3 : SelectedFilter.StartsWith("12") ? 12 : 6;
        var warna = Session.IsDark ? SKColors.WhiteSmoke : SKColor.Parse("#424242");
        SolidColorPaint P() => new(warna);

        using var db = new AppDbContext();
        var semua = db.Transaksis.Include(t => t.Kategori)
            .Where(t => t.Status == StatusPersetujuan.Disetujui).ToList();

        Saldo = semua.Sum(t => t.Jenis == JenisKas.Masuk ? t.Nominal : -t.Nominal);

        var awal = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(n - 1));
        var bulan = Enumerable.Range(0, n).Select(i => awal.AddMonths(i)).ToList();
        var label = bulan.Select(m => m.ToString("MMM yy", DataHelper.Id)).ToArray();

        double Total(DateTime m, JenisKas j) => semua
            .Where(t => t.Jenis == j && t.Tanggal.Year == m.Year && t.Tanggal.Month == m.Month)
            .Sum(t => (double)t.Nominal);

        var masuk = bulan.Select(m => Total(m, JenisKas.Masuk)).ToArray();
        var keluar = bulan.Select(m => Total(m, JenisKas.Keluar)).ToArray();

        double berjalan = semua.Where(t => t.Tanggal < awal)
            .Sum(t => t.Jenis == JenisKas.Masuk ? (double)t.Nominal : -(double)t.Nominal);
        var kumulatif = new double[n];
        for (int i = 0; i < n; i++)
        {
            berjalan += masuk[i] - keluar[i];
            kumulatif[i] = berjalan;
        }

        TotalMasuk = (long)masuk.Sum();
        TotalKeluar = (long)keluar.Sum();

        Axis[] X() => new[] { new Axis { Labels = label, LabelsPaint = P() } };
        Axis[] Y() => new[] { new Axis { Labeler = v => $"{v / 1_000_000:0.##} jt", LabelsPaint = P() } };

        // UI-01: grafik garis (tren saldo), batang (masuk vs keluar), donat (komposisi pengeluaran)
        TrenSeries = new ISeries[]
        {
            new LineSeries<double> { Name = "Saldo kumulatif", Values = kumulatif, GeometrySize = 9, Fill = null }
        };
        TrenX = X(); TrenY = Y();

        BarSeries = new ISeries[]
        {
            new ColumnSeries<double> { Name = "Pemasukan", Values = masuk },
            new ColumnSeries<double> { Name = "Pengeluaran", Values = keluar }
        };
        BarX = X(); BarY = Y();

        KomposisiSeries = semua
            .Where(t => t.Jenis == JenisKas.Keluar && t.Tanggal >= awal)
            .GroupBy(t => t.Kategori?.Nama ?? "Lainnya")
            .Select(g => (ISeries)new PieSeries<double>
            {
                Name = g.Key,
                Values = new[] { (double)g.Sum(x => x.Nominal) },
                InnerRadius = 50
            }).ToArray();

        LegendPaint = P();

        // tunggakan (anggota hanya melihat miliknya)
        var iuran = db.Iurans.Where(i => i.Status == StatusIuran.Menunggak);
        if (Session.IsAnggota) iuran = iuran.Where(i => i.AnggotaId == Session.CurrentUser.AnggotaId);
        var daftar = iuran.Select(i => i.Nominal).ToList();
        JumlahTunggakan = daftar.Count;
        TotalTunggakan = daftar.Sum();

        Notifikasis.Clear();
        foreach (var nf in NotificationService.Ambil()) Notifikasis.Add(nf);
    }
}

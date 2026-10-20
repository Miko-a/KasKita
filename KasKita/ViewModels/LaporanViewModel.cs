using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Services;
using Microsoft.Win32;

namespace KasKita.ViewModels;

public partial class LaporanViewModel : PageViewModel
{
    [ObservableProperty] private int selectedBulan = DateTime.Today.Month;
    [ObservableProperty] private int selectedTahun = DateTime.Today.Year;
    [ObservableProperty] private LaporanBulanan laporan;
    [ObservableProperty] private ObservableCollection<KasKita.Models.Transaksi> mutasi = new();

    public List<BulanItem> Bulans { get; } = Enumerable.Range(1, 12)
        .Select(i => new BulanItem(i, new DateTime(2000, i, 1).ToString("MMMM", DataHelper.Id))).ToList();
    public List<int> Tahuns { get; } = Enumerable.Range(DateTime.Today.Year - 3, 5).ToList();

    public LaporanViewModel() => Generate();

    partial void OnSelectedBulanChanged(int value) => Generate();
    partial void OnSelectedTahunChanged(int value) => Generate();

    [RelayCommand]
    private void Generate()
    {
        Laporan = ReportService.Buat(SelectedBulan, SelectedTahun);
        Mutasi = new ObservableCollection<KasKita.Models.Transaksi>(Laporan.Mutasi);
    }

    [RelayCommand]
    private void ExportPdf()
    {
        var dlg = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"Laporan_Kas_{SelectedTahun}_{SelectedBulan:00}.pdf" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            ReportService.ExportPdf(Laporan, dlg.FileName);
            LogService.Catat($"Ekspor PDF {Laporan.NamaBulan}");
            Toast.Show("Laporan PDF berhasil dibuat.");
            Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }
        catch (Exception ex) { Toast.Show("Gagal ekspor PDF: " + ex.Message); }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        var dlg = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"Laporan_Kas_{SelectedTahun}_{SelectedBulan:00}.xlsx" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            ReportService.ExportExcel(Laporan, dlg.FileName);
            LogService.Catat($"Ekspor Excel {Laporan.NamaBulan}");
            Toast.Show("Laporan Excel berhasil dibuat.");
            Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }
        catch (Exception ex) { Toast.Show("Gagal ekspor Excel: " + ex.Message); }
    }
}

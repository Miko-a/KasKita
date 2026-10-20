using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KasKita.Data;
using KasKita.Models;
using KasKita.Services;
using Microsoft.EntityFrameworkCore;

namespace KasKita.ViewModels;

public class KegiatanRow
{
    public Kegiatan Kegiatan { get; init; }
    public long Realisasi { get; init; }
    public int Id => Kegiatan.Id;
    public string Nama => Kegiatan.Nama;
    public DateTime Tanggal => Kegiatan.Tanggal;
    public StatusPersetujuan Status => Kegiatan.Status;
    public long TotalAnggaran => Kegiatan.TotalAnggaran;
    public string Catatan => Kegiatan.Catatan;
    public long Sisa => TotalAnggaran - Realisasi;
    public double Persen => TotalAnggaran > 0 ? (double)Realisasi / TotalAnggaran * 100 : 0;
    public double PersenBar => Math.Min(100, Persen);
}

public partial class AnggaranViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<KegiatanRow> items = new();
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private KegiatanRow selected;
    [ObservableProperty] private string catatanKetua = "";

    // form pengajuan
    [ObservableProperty] private string newNama = "";
    [ObservableProperty] private DateTime newTanggal = DateTime.Today.AddDays(14);
    [ObservableProperty] private string itemUraian = "";
    [ObservableProperty] private string itemJumlah = "";
    [ObservableProperty] private long draftTotal;

    public ObservableCollection<AnggaranItem> Draft { get; } = new();
    public ObservableCollection<AnggaranItem> Detail { get; } = new();

    public AnggaranViewModel() => Load();

    partial void OnSearchTextChanged(string value) => Load();

    partial void OnSelectedChanged(KegiatanRow value)
    {
        Detail.Clear();
        if (value == null) return;
        using var db = new AppDbContext();
        foreach (var i in db.AnggaranItems.Where(i => i.KegiatanId == value.Id).OrderBy(i => i.Id))
            Detail.Add(i);
    }

    private void Load()
    {
        var idTerpilih = Selected?.Id;
        using var db = new AppDbContext();
        var q = db.Kegiatans.AsQueryable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLower();
            q = q.Where(k => k.Nama.ToLower().Contains(s));
        }
        var realisasi = DataHelper.RealisasiPerKegiatan(db);
        var rows = q.OrderByDescending(k => k.Tanggal).ToList()
            .Select(k => new KegiatanRow { Kegiatan = k, Realisasi = realisasi.GetValueOrDefault(k.Id) }).ToList();
        Items = new ObservableCollection<KegiatanRow>(rows);
        Selected = rows.FirstOrDefault(r => r.Id == idTerpilih);
    }

    [RelayCommand]
    private void TambahItem()
    {
        var teks = (ItemJumlah ?? "").Replace(".", "").Replace(",", "").Trim();
        if (string.IsNullOrWhiteSpace(ItemUraian) || !long.TryParse(teks, out var jumlah) || jumlah <= 0)
        {
            Toast.Show("Isi uraian dan jumlah item dengan benar.");
            return;
        }
        Draft.Add(new AnggaranItem { Uraian = ItemUraian.Trim(), Jumlah = jumlah });
        DraftTotal = Draft.Sum(d => d.Jumlah);
        ItemUraian = "";
        ItemJumlah = "";
    }

    [RelayCommand]
    private void HapusItem(AnggaranItem item)
    {
        if (item == null) return;
        Draft.Remove(item);
        DraftTotal = Draft.Sum(d => d.Jumlah);
    }

    [RelayCommand]
    private void Ajukan()
    {
        if (!IsBendahara) return;
        if (string.IsNullOrWhiteSpace(NewNama)) { Toast.Show("Nama kegiatan wajib diisi."); return; }
        if (Draft.Count == 0) { Toast.Show("Tambahkan minimal satu rincian item."); return; }

        using var db = new AppDbContext();
        var periode = DataHelper.PeriodeAktif(db);
        var kegiatan = new Kegiatan
        {
            Nama = NewNama.Trim(), Tanggal = NewTanggal.Date, PeriodeId = periode.Id,
            Status = StatusPersetujuan.Menunggu, TotalAnggaran = Draft.Sum(d => d.Jumlah),
            Items = Draft.Select(d => new AnggaranItem { Uraian = d.Uraian, Jumlah = d.Jumlah }).ToList()
        };
        db.Kegiatans.Add(kegiatan);
        db.SaveChanges();
        LogService.Catat($"Ajukan anggaran \"{kegiatan.Nama}\" {DataHelper.Rp(kegiatan.TotalAnggaran)}");

        NewNama = "";
        Draft.Clear();
        DraftTotal = 0;
        Load();
        Toast.Show("Anggaran diajukan, menunggu persetujuan Ketua.");
    }

    public override void Simpan() => Ajukan();

    [RelayCommand] private void Setujui() => Putuskan(StatusPersetujuan.Disetujui);
    [RelayCommand] private void Tolak() => Putuskan(StatusPersetujuan.Ditolak);

    private void Putuskan(StatusPersetujuan status)
    {
        if (!IsKetua) return;
        if (Selected == null) { Toast.Show("Pilih kegiatan pada tabel terlebih dahulu."); return; }
        using var db = new AppDbContext();
        var e = db.Kegiatans.Find(Selected.Id);
        if (e == null) return;
        e.Status = status;
        e.Catatan = CatatanKetua;
        db.SaveChanges();
        LogService.Catat($"{status} anggaran \"{e.Nama}\"");
        CatatanKetua = "";
        Load();
        Toast.Show(status == StatusPersetujuan.Disetujui ? "Anggaran disetujui." : "Anggaran ditolak.");
    }
}

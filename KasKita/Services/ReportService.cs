using ClosedXML.Excel;
using KasKita.Data;
using KasKita.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KasKita.Services;

public class LaporanBulanan
{
    public int Bulan { get; set; }
    public int Tahun { get; set; }
    public long SaldoAwal { get; set; }
    public long TotalMasuk { get; set; }
    public long TotalKeluar { get; set; }
    public long SaldoAkhir => SaldoAwal + TotalMasuk - TotalKeluar;
    public List<Transaksi> Mutasi { get; set; } = new();
    public string NamaBulan => new DateTime(Tahun, Bulan, 1).ToString("MMMM yyyy", DataHelper.Id);
}

public static class ReportService
{
    public static LaporanBulanan Buat(int bulan, int tahun)
    {
        using var db = new AppDbContext();
        var awal = new DateTime(tahun, bulan, 1);
        var akhir = awal.AddMonths(1);

        var disetujui = db.Transaksis.Where(t => t.Status == StatusPersetujuan.Disetujui);
        var sebelum = disetujui.Where(t => t.Tanggal < awal);
        long masukSebelum = sebelum.Where(t => t.Jenis == JenisKas.Masuk).Sum(t => t.Nominal);
        long keluarSebelum = sebelum.Where(t => t.Jenis == JenisKas.Keluar).Sum(t => t.Nominal);

        var mutasi = disetujui.Include(t => t.Kategori)
            .Where(t => t.Tanggal >= awal && t.Tanggal < akhir)
            .OrderBy(t => t.Tanggal).ThenBy(t => t.Id).ToList();

        return new LaporanBulanan
        {
            Bulan = bulan,
            Tahun = tahun,
            SaldoAwal = masukSebelum - keluarSebelum,
            TotalMasuk = mutasi.Where(t => t.Jenis == JenisKas.Masuk).Sum(t => t.Nominal),
            TotalKeluar = mutasi.Where(t => t.Jenis == JenisKas.Keluar).Sum(t => t.Nominal),
            Mutasi = mutasi
        };
    }

    public static void ExportPdf(LaporanBulanan l, string path)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        static IContainer Head(IContainer c) => c.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.Bold());
        static IContainer Body(IContainer c) => c.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);

        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(h =>
                {
                    h.Item().Text("Laporan Keuangan Bulanan - KasKita").FontSize(16).Bold();
                    h.Item().Text($"Periode: {l.NamaBulan}").FontSize(11);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Text($"Saldo awal    : {DataHelper.Rp(l.SaldoAwal)}");
                    col.Item().Text($"Total masuk   : {DataHelper.Rp(l.TotalMasuk)}");
                    col.Item().Text($"Total keluar  : {DataHelper.Rp(l.TotalKeluar)}");
                    col.Item().Text($"Saldo akhir   : {DataHelper.Rp(l.SaldoAkhir)}").Bold();

                    col.Item().PaddingTop(10).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(70);
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.ConstantColumn(80);
                            c.ConstantColumn(80);
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(Head).Text("Tanggal");
                            h.Cell().Element(Head).Text("Kategori");
                            h.Cell().Element(Head).Text("Keterangan");
                            h.Cell().Element(Head).AlignRight().Text("Masuk");
                            h.Cell().Element(Head).AlignRight().Text("Keluar");
                        });
                        foreach (var m in l.Mutasi)
                        {
                            t.Cell().Element(Body).Text(m.Tanggal.ToString("dd/MM/yyyy"));
                            t.Cell().Element(Body).Text(m.Kategori?.Nama ?? "-");
                            t.Cell().Element(Body).Text(m.Keterangan);
                            t.Cell().Element(Body).AlignRight().Text(m.Jenis == JenisKas.Masuk ? m.Nominal.ToString("N0", DataHelper.Id) : "");
                            t.Cell().Element(Body).AlignRight().Text(m.Jenis == JenisKas.Keluar ? m.Nominal.ToString("N0", DataHelper.Id) : "");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Halaman ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf(path);
    }

    public static void ExportExcel(LaporanBulanan l, string path)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Laporan");

        ws.Cell("A1").Value = "Laporan Keuangan Bulanan - KasKita";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;
        ws.Cell("A2").Value = "Periode: " + l.NamaBulan;

        ws.Cell("A4").Value = "Saldo awal";   ws.Cell("B4").Value = l.SaldoAwal;
        ws.Cell("A5").Value = "Total masuk";  ws.Cell("B5").Value = l.TotalMasuk;
        ws.Cell("A6").Value = "Total keluar"; ws.Cell("B6").Value = l.TotalKeluar;
        ws.Cell("A7").Value = "Saldo akhir";  ws.Cell("B7").Value = l.SaldoAkhir;
        ws.Range("A7:B7").Style.Font.Bold = true;
        ws.Range("B4:B7").Style.NumberFormat.Format = "#,##0";

        int r = 9;
        string[] kepala = { "Tanggal", "Kategori", "Keterangan", "Masuk", "Keluar" };
        for (int i = 0; i < kepala.Length; i++)
        {
            ws.Cell(r, i + 1).Value = kepala[i];
            ws.Cell(r, i + 1).Style.Font.Bold = true;
            ws.Cell(r, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        foreach (var m in l.Mutasi)
        {
            r++;
            ws.Cell(r, 1).Value = m.Tanggal;
            ws.Cell(r, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Cell(r, 2).Value = m.Kategori?.Nama ?? "-";
            ws.Cell(r, 3).Value = m.Keterangan;
            if (m.Jenis == JenisKas.Masuk) ws.Cell(r, 4).Value = m.Nominal; else ws.Cell(r, 5).Value = m.Nominal;
        }
        ws.Range(10, 4, Math.Max(r, 10), 5).Style.NumberFormat.Format = "#,##0";
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
    }
}

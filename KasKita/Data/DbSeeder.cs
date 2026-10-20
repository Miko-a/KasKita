using KasKita.Models;

namespace KasKita.Data;

public static class DbSeeder
{
    public static void Init()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
        if (db.Users.Any()) return;

        var now = DateTime.Today;
        var periode = new Periode
        {
            Nama = $"Kepengurusan {now.Year}",
            TanggalMulai = new DateTime(now.Year, 1, 1),
            TanggalAkhir = new DateTime(now.Year, 12, 31)
        };

        var kIuran = new Kategori { Nama = "Iuran Anggota", Jenis = JenisKas.Masuk };
        var kSponsor = new Kategori { Nama = "Sponsor", Jenis = JenisKas.Masuk };
        var kKampus = new Kategori { Nama = "Dana Kampus", Jenis = JenisKas.Masuk };
        var kKonsumsi = new Kategori { Nama = "Konsumsi", Jenis = JenisKas.Keluar };
        var kPerlengkapan = new Kategori { Nama = "Perlengkapan", Jenis = JenisKas.Keluar };
        var kTransport = new Kategori { Nama = "Transportasi", Jenis = JenisKas.Keluar };
        var kPublikasi = new Kategori { Nama = "Publikasi", Jenis = JenisKas.Keluar };

        var nama = new[]
        {
            ("5025241001", "Bendahara Contoh", "Bendahara"),
            ("5025241002", "Ketua Contoh", "Ketua"),
            ("5025241003", "Anggota Contoh", "Anggota"),
            ("5025241004", "Rani Putri", "Anggota"),
            ("5025241005", "Dimas Pratama", "Anggota"),
            ("5025241006", "Salsa Maharani", "Anggota"),
            ("5025241007", "Fajar Nugroho", "Anggota"),
            ("5025241008", "Citra Lestari", "Anggota"),
        };
        var anggota = nama.Select(n => new Anggota { Nim = n.Item1, Nama = n.Item2, Jabatan = n.Item3 }).ToList();

        db.Periodes.Add(periode);
        db.Kategoris.AddRange(kIuran, kSponsor, kKampus, kKonsumsi, kPerlengkapan, kTransport, kPublikasi);
        db.Anggotas.AddRange(anggota);

        db.Users.AddRange(
            new User { Username = "bendahara", PasswordHash = BCrypt.Net.BCrypt.HashPassword("bendahara123"), Role = Role.Bendahara, Anggota = anggota[0] },
            new User { Username = "ketua", PasswordHash = BCrypt.Net.BCrypt.HashPassword("ketua123"), Role = Role.Ketua, Anggota = anggota[1] },
            new User { Username = "anggota", PasswordHash = BCrypt.Net.BCrypt.HashPassword("anggota123"), Role = Role.Anggota, Anggota = anggota[2] });

        var k1 = new Kegiatan
        {
            Nama = "Seminar Nasional Teknologi", Tanggal = now.AddDays(-10), Status = StatusPersetujuan.Disetujui,
            TotalAnggaran = 3_000_000, Periode = periode,
            Items =
            {
                new AnggaranItem { Uraian = "Sewa gedung", Jumlah = 1_500_000 },
                new AnggaranItem { Uraian = "Konsumsi peserta", Jumlah = 1_000_000 },
                new AnggaranItem { Uraian = "Publikasi", Jumlah = 500_000 },
            }
        };
        var k2 = new Kegiatan
        {
            Nama = "Malam Keakraban Angkatan", Tanggal = now.AddDays(20), Status = StatusPersetujuan.Menunggu,
            TotalAnggaran = 3_000_000, Periode = periode,
            Items =
            {
                new AnggaranItem { Uraian = "Sewa vila", Jumlah = 2_000_000 },
                new AnggaranItem { Uraian = "Konsumsi", Jumlah = 1_000_000 },
            }
        };
        db.Kegiatans.AddRange(k1, k2);

        var rnd = new Random(7);
        for (int i = 5; i >= 0; i--)
        {
            var b = now.AddMonths(-i);
            db.Transaksis.Add(new Transaksi { Tanggal = new DateTime(b.Year, b.Month, 5), Nominal = 300_000, Jenis = JenisKas.Masuk, Kategori = kIuran, Periode = periode, Keterangan = "Rekap iuran bulanan" });
            if (i % 2 == 0)
                db.Transaksis.Add(new Transaksi { Tanggal = new DateTime(b.Year, b.Month, 12), Nominal = 1_000_000, Jenis = JenisKas.Masuk, Kategori = kSponsor, Periode = periode, Keterangan = "Sponsor kegiatan" });
            if (i % 3 == 0)
                db.Transaksis.Add(new Transaksi { Tanggal = new DateTime(b.Year, b.Month, 10), Nominal = 2_000_000, Jenis = JenisKas.Masuk, Kategori = kKampus, Periode = periode, Keterangan = "Dana pembinaan kampus" });

            var keluar = new[] { (kKonsumsi, "Konsumsi rapat"), (kPerlengkapan, "Pembelian ATK"), (kTransport, "Transport kegiatan"), (kPublikasi, "Cetak poster") };
            for (int j = 0; j < 2; j++)
            {
                var (kat, ket) = keluar[rnd.Next(keluar.Length)];
                db.Transaksis.Add(new Transaksi
                {
                    Tanggal = new DateTime(b.Year, b.Month, 15 + j * 5), Nominal = rnd.Next(2, 10) * 50_000L,
                    Jenis = JenisKas.Keluar, Kategori = kat, Periode = periode, Keterangan = ket
                });
            }
        }
        db.Transaksis.AddRange(
            new Transaksi { Tanggal = now.AddDays(-8), Nominal = 800_000, Jenis = JenisKas.Keluar, Kategori = kKonsumsi, Periode = periode, Kegiatan = k1, Keterangan = "Konsumsi seminar" },
            new Transaksi { Tanggal = now.AddDays(-7), Nominal = 300_000, Jenis = JenisKas.Keluar, Kategori = kPublikasi, Periode = periode, Kegiatan = k1, Keterangan = "Cetak banner seminar" },
            new Transaksi { Tanggal = now.AddDays(-1), Nominal = 1_200_000, Jenis = JenisKas.Keluar, Kategori = kPerlengkapan, Periode = periode, Kegiatan = k1, Keterangan = "Sewa sound system", Status = StatusPersetujuan.Menunggu });

        for (int i = 2; i >= 0; i--)
        {
            var b = now.AddMonths(-i);
            for (int a = 0; a < anggota.Count; a++)
            {
                bool lunas = i > 0 ? a != 4 : a < 4;
                db.Iurans.Add(new Iuran
                {
                    Anggota = anggota[a], Periode = periode, Bulan = b.Month, Tahun = b.Year, Nominal = Konstanta.NominalIuranDefault,
                    Status = lunas ? StatusIuran.Lunas : StatusIuran.Menunggak,
                    TanggalBayar = lunas ? new DateTime(b.Year, b.Month, 5) : null
                });
            }
        }

        db.SaveChanges();
    }
}

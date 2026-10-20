# KasKita

**Aplikasi Desktop Manajemen Keuangan Organisasi Mahasiswa**  
Laporan Proyek Evaluasi Tengah Semester (ETS)  
Mata Kuliah Pemrograman Berbasis Kerangka Kerja (PBKK)

## Identitas Proyek

- **Institusi:** Departemen Teknik Informatika, Institut Teknologi Sepuluh Nopember
- **Platform:** Desktop Windows
- **Framework:** .NET 8 dan Windows Presentation Foundation (WPF)
- **Bahasa pemrograman:** C#
- **Basis data:** SQLite

### Tim Pengembang

- Agil Lukman Hakim Muchdi (5025241037)
- [Nama Anggota 2] ([NRP])
- [Nama Anggota 3] ([NRP])

## Ringkasan

KasKita adalah aplikasi desktop untuk membantu pengurus organisasi mahasiswa mencatat dan memantau keuangan organisasi. Aplikasi menyediakan pengelolaan transaksi kas, iuran anggota, anggaran kegiatan, dan laporan keuangan dalam satu aplikasi. Data disimpan secara lokal menggunakan SQLite, sedangkan laporan bulanan dapat diekspor ke PDF dan Excel.

Aplikasi menyediakan tiga peran pengguna, yaitu Bendahara, Ketua, dan Anggota. Antarmuka berbahasa Indonesia dirancang agar aktivitas pencatatan dan pemantauan keuangan dapat dilakukan melalui halaman-halaman yang terpisah sesuai modulnya.

## Latar Belakang dan Tujuan

Pencatatan keuangan yang tersebar di buku, percakapan, atau spreadsheet dapat menyulitkan pengurus dalam menelusuri transaksi, memantau iuran, membandingkan anggaran dengan realisasi, dan menyusun laporan. KasKita dikembangkan sebagai sarana pencatatan terpusat yang memudahkan pengurus melihat kondisi kas dan menyiapkan laporan.

Tujuan proyek:

1. Menyediakan pencatatan pemasukan dan pengeluaran yang terstruktur.
2. Membantu bendahara memantau pembayaran iuran anggota.
3. Mendukung pengelolaan serta pemantauan anggaran kegiatan.
4. Menampilkan ringkasan kondisi keuangan melalui dashboard.
5. Menghasilkan laporan keuangan bulanan dalam format PDF dan Excel.

## Fitur Aplikasi

- **Login dan peran pengguna:** autentikasi menggunakan kata sandi yang di-hash dengan BCrypt.
- **Dashboard:** ringkasan keuangan dan visualisasi data transaksi.
- **Transaksi kas:** pencatatan pemasukan dan pengeluaran, kategori transaksi, keterangan, tanggal, periode, serta bukti transaksi.
- **Persetujuan:** transaksi pengeluaran tertentu dapat menunggu persetujuan Ketua; aplikasi menetapkan ambang persetujuan sebesar Rp500.000.
- **Iuran anggota:** pencatatan iuran per anggota dan periode, dengan status lunas atau menunggak.
- **Anggaran kegiatan:** pencatatan kegiatan dan rincian anggaran serta pemantauan realisasi pengeluaran yang disetujui.
- **Data master:** pengelolaan anggota, periode, dan kategori kas.
- **Laporan keuangan:** perhitungan saldo awal, total pemasukan, total pengeluaran, saldo akhir, dan daftar mutasi per bulan.
- **Ekspor laporan:** pembuatan dokumen PDF dan spreadsheet Excel.
- **Pencadangan data:** layanan untuk mencadangkan dan memulihkan berkas basis data SQLite.

## Peran Pengguna

| Peran | Tanggung jawab utama |
| --- | --- |
| Bendahara | Mengelola transaksi, iuran, data master, anggaran, dan laporan keuangan. |
| Ketua | Memantau kondisi keuangan serta meninjau transaksi atau anggaran yang memerlukan persetujuan. |
| Anggota | Melihat informasi yang berkaitan dengan keanggotaan dan status iuran. |

Hak akses yang tersedia pada aplikasi dapat berbeda sesuai peran yang sedang digunakan.

## Teknologi dan Arsitektur

| Komponen | Teknologi |
| --- | --- |
| Aplikasi desktop | WPF, XAML, .NET 8, C# |
| Pola presentasi | MVVM dengan CommunityToolkit.Mvvm |
| Basis data dan ORM | SQLite dan Entity Framework Core |
| Komponen tampilan | MaterialDesignInXAML |
| Visualisasi data | LiveCharts2 |
| Ekspor Excel | ClosedXML |
| Ekspor PDF | QuestPDF |
| Hash kata sandi | BCrypt.Net |

Kode dipisahkan ke dalam folder `Views`, `ViewModels`, `Models`, `Data`, `Services`, dan `Resources`. View menampilkan antarmuka WPF, ViewModel menghubungkan tampilan dengan operasi aplikasi, Model menyimpan entitas domain, Data mengatur konteks dan inisialisasi basis data, sedangkan Services menangani kebutuhan seperti autentikasi, laporan, notifikasi, dan pencadangan.

## Struktur Proyek

```text
KasKita/
├── KasKita.sln
├── README.md
└── KasKita/
    ├── KasKita.csproj
    ├── App.xaml
    ├── Data/
    ├── Models/
    ├── Resources/
    ├── Services/
    ├── ViewModels/
    └── Views/
```

## Persyaratan Sistem

- Windows 10 atau Windows 11.
- .NET 8 SDK untuk membangun dan menjalankan proyek dari kode sumber.
- Visual Studio 2022 dengan workload **.NET desktop development**, atau .NET CLI.

## Menjalankan Aplikasi

Jalankan perintah berikut dari direktori repositori yang berisi `KasKita.sln`:

```powershell
dotnet restore .\KasKita\KasKita.csproj
dotnet run --project .\KasKita\KasKita.csproj
```

Saat dijalankan pertama kali, aplikasi membuat basis data dan data contoh secara otomatis jika belum ada pengguna yang tersimpan. Basis data menggunakan SQLite dan berada di:

```text
%LOCALAPPDATA%\KasKita\kaskita.db
```

Bukti transaksi yang disalin oleh aplikasi disimpan di subfolder `Bukti` pada direktori data KasKita tersebut.

## Akun Data Contoh

Data awal menyediakan akun untuk mencoba setiap peran berikut:

| Peran | Username | Password |
| --- | --- | --- |
| Bendahara | `bendahara` | `bendahara123` |
| Ketua | `ketua` | `ketua123` |
| Anggota | `anggota` | `anggota123` |

Akun dan kata sandi ini hanya ditujukan untuk demonstrasi lokal. Ganti atau hapus data contoh sebelum menggunakan aplikasi untuk data nyata.

## Entitas Data

Basis data memuat entitas utama berikut:

- **User** dan **Anggota** untuk akun, peran, dan data anggota.
- **Periode** untuk periode kepengurusan.
- **Kategori** dan **Transaksi** untuk pencatatan arus kas.
- **Iuran** untuk kewajiban dan pembayaran anggota.
- **Kegiatan** dan **AnggaranItem** untuk anggaran dan realisasi kegiatan.
- **LogAktivitas** untuk pencatatan aktivitas aplikasi.

## Batasan Cakupan

- Aplikasi ditujukan untuk penggunaan desktop Windows.
- Data disimpan di perangkat lokal; sinkronisasi cloud dan penggunaan lintas perangkat tidak disediakan.
- Integrasi pembayaran melalui bank atau payment gateway tidak termasuk dalam cakupan proyek.
- Data contoh yang dibuat saat inisialisasi ditujukan untuk kebutuhan demonstrasi.

## Informasi Laporan

Proyek ini disusun sebagai bagian dari Evaluasi Tengah Semester mata kuliah Pemrograman Berbasis Kerangka Kerja (PBKK), Departemen Teknik Informatika, Institut Teknologi Sepuluh Nopember, Surabaya.

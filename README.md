# KasKita

## Mengelola Keuangan Organisasi Mahasiswa dengan Lebih Mudah, Transparan, dan Terstruktur

KasKita adalah aplikasi desktop berbasis Windows yang dirancang untuk membantu organisasi mahasiswa dalam mengelola keuangan secara lebih rapi, efisien, dan akuntabel. Aplikasi ini dikembangkan dengan pendekatan Rich UI menggunakan WPF (.NET 8) dan C#, sehingga tampilannya modern, responsif, dan mudah digunakan bahkan oleh pengguna yang tidak terbiasa dengan sistem keuangan yang kompleks.

Dengan fokus pada kebutuhan pengelola organisasi mahasiswa, KasKita hadir sebagai solusi digital untuk mengatasi masalah umum seperti pencatatan kas yang tersebar, sulitnya memantau iuran anggota, serta minimnya transparansi dalam pengelolaan anggaran kegiatan. Aplikasi ini tidak hanya berfungsi sebagai alat pencatat transaksi, tetapi juga sebagai dashboard untuk memantau kondisi keuangan organisasi secara realtime.


## 1. Deskripsi Aplikasi dan Ruang Lingkup Fitur

KasKita merupakan aplikasi manajemen keuangan organisasi mahasiswa yang membantu bendahara, ketua, dan anggota dalam mengelola seluruh aspek kas organisasi mulai dari pencatatan transaksi, pembayaran iuran, hingga pelaporan keuangan dan anggaran kegiatan. Aplikasi ini dibangun dengan antarmuka berbahasa Indonesia agar lebih mudah dipahami dan digunakan oleh komunitas kampus.

### Ruang Lingkup Fitur Utama

#### a. Autentikasi dan Manajemen Peran
- Login berbasis akun pengguna
- Sistem role-based access untuk tiga peran utama:
  - Bendahara
  - Ketua
  - Anggota
- Keamanan kata sandi menggunakan hashing BCrypt, sehingga data login lebih aman

#### b. Dashboard Keuangan
- Ringkasan kondisi keuangan organisasi
- Informasi saldo terkini, pemasukan, pengeluaran, dan tren keuangan
- Visualisasi data transaksi agar pengurus dapat membaca kondisi keuangan dengan cepat

#### c. Pencatatan Transaksi Kas
- Input pemasukan dan pengeluaran kas organisasi
- Kategori transaksi yang dapat diatur sesuai kebutuhan
- Informasi seperti tanggal, periode, keterangan, nominal, dan bukti transaksi
- Proses persetujuan khusus untuk pengeluaran tertentu dengan ambang batas tertentu

#### d. Pengelolaan Iuran Anggota
- Pencatatan iuran per anggota per periode
- Status pembayaran: lunas atau menunggak
- Mempermudah bendahara dalam memantau kewajiban anggota secara terstruktur

#### e. Manajemen Anggaran Kegiatan
- Pencatatan kegiatan organisasi
- Rincian anggaran yang direncanakan
- Pemantauan realisasi penggunaan anggaran
- Membantu pengurus memastikan kegiatan sesuai dengan alokasi dana

#### f. Data Master Organisasi
- Pengelolaan data anggota
- Pengelolaan data periode kepengurusan
- Pengelolaan kategori transaksi dan kas
- Menyediakan fondasi data yang konsisten untuk seluruh aktivitas organisasi

#### g. Laporan Keuangan
- Menghitung saldo awal, total pemasukan, total pengeluaran, dan saldo akhir
- Menampilkan daftar mutasi per bulan
- Menyediakan analisis ringkas untuk keperluan evaluasi organisasi

#### h. Ekspor Laporan
- Export laporan ke PDF
- Export ke format Excel/Spreadsheet
- Memudahkan pengurus untuk dibagikan ke pihak yang berkepentingan

#### i. Backup dan Restore Data
- Fitur pencadangan basis data SQLite
- Proses pemulihan data menjadi lebih mudah dan aman
- Mengurangi risiko kehilangan data akibat human error atau masalah perangkat

### Fokus Pengguna
KasKita dirancang untuk mendukung kebutuhan pengurus organisasi mahasiswa dalam menjalankan tugas administratif keuangan secara lebih profesional. Aplikasi ini tidak hanya berguna untuk bendahara, tetapi juga membantu ketua dalam memantau kondisi keuangan dan anggota dalam melihat status keanggotaan serta kewajiban iuran mereka.


## 2. Latar Belakang Pembuatan Aplikasi

Pada banyak organisasi mahasiswa, pengelolaan keuangan masih sering dilakukan secara manual. Data transaksi dicatat pada buku kas, spreadsheet, atau bahkan hanya melalui chat grup. Kondisi ini menimbulkan beberapa masalah, antara lain:

- transaksi tidak terdokumentasi dengan rapi
- sulitnya melacak pemasukan dan pengeluaran secara historis
- keterlambatan dalam pengecekan status iuran anggota
- kurangnya transparansi antara bendahara dan pengurus lainnya
- kesulitan dalam membuat laporan keuangan saat periode evaluasi

Fenomena ini mendorong kebutuhan akan sistem yang dapat mengelola keuangan organisasi secara digital, terstruktur, dan mudah diaudit. Oleh karena itu, KasKita dibuat sebagai solusi yang sederhana namun efektif untuk mengelola keuangan organisasi mahasiswa di lingkungan kampus.


## 3. Manfaat Aplikasi

KasKita memberikan berbagai manfaat yang signifikan, baik dari sisi operasional maupun pengambilan keputusan organisasi.

### Manfaat utama:

1. Meningkatkan transparansi keuangan
   - Semua transaksi tercatat dengan jelas dan mudah dipantau.

2. Menghemat waktu administrasi
   - Proses pencatatan, pengecekan, dan pembuatan laporan menjadi lebih cepat.

3. Meningkatkan akuntabilitas pengurus
   - Setiap transaksi dapat ditelusuri dan dipertanggungjawabkan.

4. Mempermudah pengambilan keputusan
   - Dashboard dan laporan membantu ketua dan bendahara membangun keputusan berdasarkan data nyata.

5. Mengurangi risiko kesalahan manual
   - Sistem terstruktur meminimalkan celah human error dalam pencatatan keuangan.

6. Mendukung pengelolaan kegiatan organisasi
   - Anggaran kegiatan dapat direncanakan, diawasi, dan dievaluasi dengan lebih baik.

7. Meningkatkan profesionalisme organisasi
   - Organisasi menjadi lebih siap dalam mengelola keuangan dengan cara yang modern dan terukur.


## 4. Teknologi yang Digunakan

KasKita dibangun dengan stack teknologi yang relevan untuk aplikasi desktop rich UI:

| Komponen | Teknologi |
| --- | --- |
| Aplikasi Desktop | WPF, XAML, .NET 8, C# |
| Pola arsitektur | MVVM dengan CommunityToolkit.Mvvm |
| Basis data | SQLite |
| ORM | Entity Framework Core |
| UI Design | MaterialDesignInXAML |
| Visualisasi data | LiveCharts2 |
| Ekspor Excel | ClosedXML |
| Ekspor PDF | QuestPDF |
| Hash password | BCrypt.Net |

Struktur kode dalam proyek dipisahkan ke folder seperti `Views`, `ViewModels`, `Models`, `Data`, `Services`, dan `Resources`, agar pengelolaan aplikasi lebih modular, terorganisir, dan mudah dikembangkan di masa depan.


## 5. Identitas Proyek

- Institusi: Departemen Teknik Informatika, Institut Teknologi Sepuluh Nopember
- Platform: Desktop Windows
- Bahasa pemrograman: C#
- Framework: .NET 8 dan WPF
- Basis data: SQLite

### Tim Pengembang
- Agil Lukman Hakim Muchdi (5025241037)
- Berwyn Rafif Alvaro (5025241029)
- Farrel Jatmiko Aji (5025241193)

Demo aplikasi: https://youtu.be/mV4gYxfGaKI


## 6. Struktur Proyek

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


## 7. Persyaratan Sistem

- Windows 10 atau Windows 11
- .NET 8 SDK
- Visual Studio 2022 dengan workload .NET desktop development, atau .NET CLI


## 8. Cara Menjalankan Aplikasi

Jalankan perintah berikut dari direktori repositori yang berisi `KasKita.sln`:

```powershell
dotnet restore .\KasKita\KasKita.csproj
dotnet run --project .\KasKita\KasKita.csproj
```

Saat dijalankan pertama kali, aplikasi akan membuat database SQLite dan data contoh secara otomatis jika belum ada pengguna tersimpan. Database default berada di:

```text
%LOCALAPPDATA%\KasKita\kaskita.db
```

Bukti transaksi yang diunggah juga akan disimpan di subfolder `Bukti` di direktori data KasKita tersebut.


## 9. Akun Demo

Aplikasi menyediakan data contoh untuk mencoba tiap peran berikut:

| Peran | Username | Password |
| --- | --- | --- |
| Bendahara | `bendahara` | `bendahara123` |
| Ketua | `ketua` | `ketua123` |
| Anggota | `anggota` | `anggota123` |

Akun ini hanya digunakan untuk demonstrasi lokal. Sebaiknya ganti atau hapus data demo sebelum digunakan untuk data organisasi yang sebenarnya.


## 10. Kesimpulan

KasKita hadir sebagai solusi digital yang relevan untuk kebutuhan pengelolaan keuangan organisasi mahasiswa. Dengan pendekatan Rich UI, fitur yang terintegrasi, dan proses yang mudah dipahami, aplikasi ini membantu organisasi menjadi lebih tertib, transparan, dan profesional dalam mengelola dana kegiatan. Lebih dari sekadar aplikasi pencatat keuangan, KasKita adalah langkah menuju tata kelola organisasi yang lebih modern dan akuntabel.

---

Jika Anda tertarik untuk melihat proyek ini lebih lanjut, jangan ragu untuk membuka repository ini dan mencoba aplikasi secara lokal.

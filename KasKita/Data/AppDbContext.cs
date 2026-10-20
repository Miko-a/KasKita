using KasKita.Models;
using Microsoft.EntityFrameworkCore;

namespace KasKita.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Anggota> Anggotas => Set<Anggota>();
    public DbSet<Periode> Periodes => Set<Periode>();
    public DbSet<Kategori> Kategoris => Set<Kategori>();
    public DbSet<Transaksi> Transaksis => Set<Transaksi>();
    public DbSet<Iuran> Iurans => Set<Iuran>();
    public DbSet<Kegiatan> Kegiatans => Set<Kegiatan>();
    public DbSet<AnggaranItem> AnggaranItems => Set<AnggaranItem>();
    public DbSet<LogAktivitas> Logs => Set<LogAktivitas>();

    public static string DbPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KasKita", "kaskita.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}

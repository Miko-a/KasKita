using KasKita.Data;
using Microsoft.Data.Sqlite;

namespace KasKita.Services;

public static class BackupService
{
    public static void Backup(string tujuan)
    {
        SqliteConnection.ClearAllPools();
        File.Copy(AppDbContext.DbPath, tujuan, true);
    }

    public static void Restore(string sumber)
    {
        SqliteConnection.ClearAllPools();
        File.Copy(sumber, AppDbContext.DbPath, true);
    }
}

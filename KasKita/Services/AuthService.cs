using KasKita.Data;
using KasKita.Models;
using Microsoft.EntityFrameworkCore;

namespace KasKita.Services;

public static class AuthService
{
    public static User Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return null;
        using var db = new AppDbContext();
        var user = db.Users.Include(u => u.Anggota).FirstOrDefault(u => u.Username == username.Trim());
        return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }
}

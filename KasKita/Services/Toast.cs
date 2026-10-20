using MaterialDesignThemes.Wpf;

namespace KasKita.Services;

/// <summary>Toast / snackbar global (UI-05).</summary>
public static class Toast
{
    public static SnackbarMessageQueue Queue { get; } = new(TimeSpan.FromSeconds(3));
    public static void Show(string pesan) => Queue.Enqueue(pesan);
}

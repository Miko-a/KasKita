using CommunityToolkit.Mvvm.ComponentModel;
using KasKita.Services;

namespace KasKita.ViewModels;

/// <summary>Basis semua halaman: izin peran dan hook pintasan keyboard (UI-08).</summary>
public abstract class PageViewModel : ObservableObject
{
    public event Action FocusSearchRequested;
    public event Action FocusFormRequested;

    public void RequestFocusSearch() => FocusSearchRequested?.Invoke();
    public void RequestFocusForm() => FocusFormRequested?.Invoke();
    public virtual void Simpan() { }

    public bool IsBendahara => Session.IsBendahara;
    public bool IsKetua => Session.IsKetua;
    public bool IsAnggota => Session.IsAnggota;
    public bool IsReadOnly => !Session.IsBendahara;
}

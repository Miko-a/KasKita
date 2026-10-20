using System.Windows;
using System.Windows.Input;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    // UI-08: Ctrl+N (tambah), Ctrl+F (cari), Ctrl+S (simpan), Ctrl+D (ganti tema)
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control || DataContext is not MainViewModel vm) return;

        switch (e.Key)
        {
            case Key.N: vm.CurrentPage?.RequestFocusForm(); e.Handled = true; break;
            case Key.F: vm.CurrentPage?.RequestFocusSearch(); e.Handled = true; break;
            case Key.S: vm.CurrentPage?.Simpan(); e.Handled = true; break;
            case Key.D: vm.IsDark = !vm.IsDark; e.Handled = true; break;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using KasKita.Models;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class TransaksiView : UserControl
{
    public TransaksiView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PageViewModel vm)
            {
                vm.FocusSearchRequested += () => SearchBox.Focus();
                vm.FocusFormRequested += () => NominalBox.Focus();
            }
        };
    }

    private void Bukti_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Bukti_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0 && DataContext is TransaksiViewModel vm) vm.NewBuktiPath = files[0];
    }

    private void Grid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit || e.Row.Item is not Transaksi t) return;
        // simpan setelah nilai sel benar-benar ter-commit
        Dispatcher.BeginInvoke(new Action(() => (DataContext as TransaksiViewModel)?.SimpanEdit(t)), DispatcherPriority.Background);
    }
}

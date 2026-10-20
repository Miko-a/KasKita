using System.Windows.Controls;
using System.Windows.Threading;
using KasKita.Models;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class MasterView : UserControl
{
    public MasterView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PageViewModel vm) vm.FocusSearchRequested += () => SearchBox.Focus();
        };
    }

    private void Anggota_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit || e.Row.Item is not Anggota a) return;
        Dispatcher.BeginInvoke(new Action(() => (DataContext as MasterViewModel)?.SimpanAnggota(a)), DispatcherPriority.Background);
    }
}

using System.Windows;
using System.Windows.Controls;
using KasKita.Services;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class IuranView : UserControl
{
    public IuranView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PageViewModel vm) vm.FocusSearchRequested += () => SearchBox.Focus();
        };
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        // hanya Bendahara yang dapat menandai pembayaran
        if (!Session.IsBendahara) AksiColumn.Visibility = Visibility.Collapsed;
    }
}

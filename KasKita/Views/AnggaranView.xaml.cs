using System.Windows.Controls;
using KasKita.ViewModels;

namespace KasKita.Views;

public partial class AnggaranView : UserControl
{
    public AnggaranView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is PageViewModel vm)
            {
                vm.FocusSearchRequested += () => SearchBox.Focus();
                vm.FocusFormRequested += () => NamaBox.Focus();
            }
        };
    }
}

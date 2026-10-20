using System.Windows.Controls;

namespace KasKita.Views;

public partial class ConfirmDialog : UserControl
{
    public ConfirmDialog(string pesan)
    {
        InitializeComponent();
        PesanText.Text = pesan;
    }
}

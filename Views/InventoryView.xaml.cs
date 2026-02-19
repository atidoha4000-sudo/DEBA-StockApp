using System.Windows.Controls;
using DEBA.StockApp.ViewModels;

namespace DEBA.StockApp.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
            DataContext = new InventoryViewModel();
        }
    }
}

using System.Windows;
using TinkoffPriceMonitor.ViewModels;

namespace TinkoffPriceMonitor.Views.Windows
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}

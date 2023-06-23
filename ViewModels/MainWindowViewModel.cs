using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkoffPriceMonitor.Models;
using TinkoffPriceMonitor.ViewModels.BaseView;

namespace TinkoffPriceMonitor.ViewModels
{

    public class MainWindowViewModel : BaseViewModel
    {

        public ObservableCollection<PriceChangeMessage> PriceChangeMessages { get; } = new ObservableCollection<PriceChangeMessage>();


        public MainWindowViewModel()
        {

        }
    }
}

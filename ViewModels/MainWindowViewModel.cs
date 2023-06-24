using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffPriceMonitor.ApiServices;
using TinkoffPriceMonitor.Models;
using TinkoffPriceMonitor.ViewModels.BaseView;

namespace TinkoffPriceMonitor.ViewModels
{

    public class MainWindowViewModel : BaseViewModel
    {
        #region Приватные свойства
        private InvestApiClient? _client = null;
        private ObservableCollection<PriceChangeMessage> _priceChangeMessages = null;
        #endregion

        #region Публичные свойства
        public ObservableCollection<PriceChangeMessage> PriceChangeMessages
        {
            get => _priceChangeMessages;
            set => Set(ref _priceChangeMessages, value);
        }
        #endregion

        private ObservableCollection<TickerGroup> _tickerGroups;

        public ObservableCollection<TickerGroup> TickerGroups
        {
            get => _tickerGroups;
            set => Set(ref _tickerGroups, value);
        }

        public MainWindowViewModel()
        {
            TickerGroups = new ObservableCollection<TickerGroup>();


        }

        public void AddTickerGroup()
        {
            TickerGroup newGroup = new TickerGroup
            {
                GroupName = "Group 1",
                Tickers = new List<string>
                {
                    "AAPL",
                    "GOOGL",
                    "MSFT"
                },
                PercentageThreshold = 0.05,
                Interval = 60
            };


            TickerGroups.Add(newGroup);
        }


    }
}

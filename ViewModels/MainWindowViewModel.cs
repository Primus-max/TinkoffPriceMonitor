using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private ObservableCollection<TrackedTickerInfo> _priceChangeMessages = null;
        private TickerPriceStorage _tickerPriceStorage;
        private ObservableCollection<TickerGroup> _tickerGroups;
        private bool _IsPositivePriceChange = false;
        //private ObservableCollection<TrackedTickerInfo> _priceChangeItems;
        #endregion

        #region Публичные свойства
        public ObservableCollection<TrackedTickerInfo> PriceChangeMessages
        {
            get => _priceChangeMessages;
            set => Set(ref _priceChangeMessages, value);
        }

        public ObservableCollection<TickerGroup> TickerGroups
        {
            get => _tickerGroups;
            set => Set(ref _tickerGroups, value);
        }

        public bool IsPositivePriceChange
        {
            get => _IsPositivePriceChange;
            set => Set(ref _IsPositivePriceChange, value);
        }
        //private ObservableCollection<TrackedTickerInfo> PriceChangeItems
        //{
        //    get => _priceChangeItems;
        //    set => Set(ref _priceChangeItems, value);
        //}
        #endregion


        public MainWindowViewModel()
        {

            #region Инициализация источников данных
            // Инициализация источника данных для отображения (настройки)
            TickerGroups = new ObservableCollection<TickerGroup>();

            // Инициализация источника данных для отображения (информация по тикерам)
            PriceChangeMessages = new ObservableCollection<TrackedTickerInfo>();
            #endregion

            #region Подписка на события
            // В конструкторе или методе инициализации MainWindowViewModel:
            //MonitorThread monitorThread = new MonitorThread();
            //monitorThread.PriceChangeSignal += MonitorThread_PriceChangeSignal;
            #endregion

            #region Вызовы методов            
            LoadTickerGroups();
            Initialize();
            //LoadSavedData();
            // AddTickerGroup();
            RunPriceMonitoring();
            #endregion
        }


        public class Candle
        {
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
        }


        // Метод для запуска мониторинга цен для всех групп тикеров
        private async void RunPriceMonitoring()
        {
            foreach (var group in TickerGroups)
            {
                MonitorThread monitor = new(group, _client);
                monitor.PriceChangeSignal += MonitorThread_PriceChangeSignal;

                await monitor.StartMonitoringAsync();

                Thread.Sleep(10000);
                //await MonitorTickerGroup(group);
            }
        }

        #region Подписчики на события
        private void MonitorThread_PriceChangeSignal(TrackedTickerInfo trackedTickerInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Проверяем, существует ли элемент с таким же именем тикера и группой в коллекции
                var existingItem = PriceChangeMessages.FirstOrDefault(item => item.TickerName == trackedTickerInfo.TickerName && item.GroupName == trackedTickerInfo.GroupName);

                if (existingItem != null)
                {
                    // Обновляем существующий элемент новыми данными
                    existingItem.IsPositivePriceChange = trackedTickerInfo.IsPositivePriceChange;
                    existingItem.PriceChangePercentage = trackedTickerInfo.PriceChangePercentage;
                    existingItem.GroupName = trackedTickerInfo.GroupName;
                }
                else
                {
                    // Добавляем новый элемент в коллекцию
                    PriceChangeMessages.Add(trackedTickerInfo);
                }
            });
        }




        #endregion

        #region Методы
        // Получаю и возвращаю инструмент по имени тикера из API
        private async Task<Share> GetShareByTicker(string ticker)
        {
            Share share = new Share();
            try
            {
                SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
                share = sharesResponse?.Instruments?.FirstOrDefault(x => x.Ticker == ticker) ?? new Share();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось получить инструмент. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return share;
        }

        // Метод инициализации клиента и некоторых методов при старте программы
        private async Task Initialize()
        {
            _client = await Creaters.CreateClientAsync();
            //await LoadTickerPricesAsync();
        }

        // Метод загрузки и сохранения цен в барном файле для отображения и дальнейшего сравнения
        //private async Task LoadTickerPricesAsync()
        //{
        //    var tickerGroups = new List<TickerPriceStorage.TickerGroup>();

        //    foreach (var group in TickerGroups)
        //    {
        //        if (group is null) continue;

        //        var tickers = group.Tickers?.Split('|');
        //        if (tickers is null) continue;

        //        var tickerGroup = new TickerPriceStorage.TickerGroup
        //        {
        //            GroupName = group.GroupName,
        //            Tickers = new List<TickerPriceStorage.TickerPrice>()
        //        };

        //        foreach (var ticker in tickers)
        //        {
        //            try
        //            {
        //                SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
        //                var instrument = sharesResponse?.Instruments?.FirstOrDefault(x => x.Ticker == ticker);

        //                if (instrument != null)
        //                {
        //                    decimal price = await Getters.GetUpdatedPrice(instrument, _client);
        //                    //tickerGroup.Tickers.Add(new TickerPriceStorage.TickerPrice { Ticker = ticker, Price = price });
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show($"Не удалось загрузить данные для тикера {ticker}. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //        tickerGroups.Add(tickerGroup);
        //    }

        //    var tickerPriceStorage = new TickerPriceStorage();
        //    tickerPriceStorage.SaveTickerPrice(tickerGroups);
        //}

        // Метод добавления группы тикеров во View (отображение)
        public void AddTickerGroup()
        {
            _tickerPriceStorage = new TickerPriceStorage();

            TickerGroup newGroup = new();

            TickerGroups.Add(newGroup);
        }

        // Метод сохранения данных полученных из текстовых полей View (главного окна) от пользователя
        public void SaveDataToJson()
        {
            // Сериализация TickerGroups в JSON
            string jsonData = JsonConvert.SerializeObject(TickerGroups);

            // Получение пути к файлу в корне программы
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");

            try
            {
                if (!File.Exists(filePath))
                {
                    // Создание нового файла, если он не существует
                    File.Create(filePath).Close();
                }

                // Запись JSON данных в файл
                File.WriteAllText(filePath, jsonData);

                MessageBox.Show("Данные сохранены в JSON файл.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось записать данные в файл data.json. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод загрузки данных в источник данных для отображения во View (главного окна)
        public void LoadTickerGroups()
        {
            string filePath = "data.json";

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                try
                {
                    List<TickerGroup> groups = JsonConvert.DeserializeObject<List<TickerGroup>>(json);
                    TickerGroups = new ObservableCollection<TickerGroup>(groups);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Не удалось загрузить данные из файла {filePath}. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                //MessageBox.Show($"Файл {filePath} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод загрузки и сохранения цен для тикеров из биржи (сохраняем в бинарнике)
        //private void LoadSavedData()
        //{
        //    TickerPriceStorage tickerPriceStorage = new();

        //    List<(string GroupName, TickerPriceStorage.TickerPrice Ticker)> savedData = tickerPriceStorage.LoadTickerPrice();
        //    foreach (var (groupName, ticker) in savedData)
        //    {
        //        TrackedTickerInfo info = new TrackedTickerInfo
        //        {
        //            GroupName = groupName,
        //            TickerName = ticker.Ticker,
        //            Price = ticker.Price,
        //            PriceChangePercentage = 0, // Установите нужное значение
        //            EventTime = DateTime.Now // Установите нужное значение
        //        };
        //        PriceChangeMessages.Add(info);
        //    }
        //}
        #endregion
    }
}

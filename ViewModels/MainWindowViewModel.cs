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
        private ObservableCollection<TrackedTickerInfo> _priceChangeMessages = null;
        private TickerPriceStorage _tickerPriceStorage;
        private ObservableCollection<TickerGroup> _tickerGroups;
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

        //private ObservableCollection<TrackedTickerInfo> PriceChangeItems
        //{
        //    get => _priceChangeItems;
        //    set => Set(ref _priceChangeItems, value);
        //}
        #endregion


        public MainWindowViewModel()
        {
            // Инициализация источника данных для отображения (настройки)
            TickerGroups = new ObservableCollection<TickerGroup>();

            // Инициализация источника данных для отображения (информация по тикерам)
            PriceChangeMessages = new ObservableCollection<TrackedTickerInfo>();

            #region Вызовы методов
            LoadTickerGroups();
            Initialize();
            LoadSavedData();
            #endregion
        }

        // Метод инициализации клиента и некоторых методов при старте программы
        private async Task Initialize()
        {
            _client = await Creaters.CreateClientAsync();
            await LoadTickerPricesAsync();
        }

        // Метод загрузки и сохранения цен в барном файле для отображения и дальнейшего сравнения
        private async Task LoadTickerPricesAsync()
        {
            var tickerGroups = new List<TickerPriceStorage.TickerGroup>();

            foreach (var group in TickerGroups)
            {
                var tickers = group.Tickers.Split('|');

                var tickerGroup = new TickerPriceStorage.TickerGroup
                {
                    GroupName = group.GroupName,
                    Tickers = new List<TickerPriceStorage.TickerPrice>()
                };

                foreach (var ticker in tickers)
                {
                    SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
                    var instrument = sharesResponse?.Instruments.FirstOrDefault(x => x.Ticker == ticker);

                    if (instrument != null)
                    {
                        decimal price = await Getters.GetUpdatedPrice(instrument, _client);
                        tickerGroup.Tickers.Add(new TickerPriceStorage.TickerPrice { Ticker = ticker, Price = price });
                    }
                }

                tickerGroups.Add(tickerGroup);
            }

            var tickerPriceStorage = new TickerPriceStorage();
            tickerPriceStorage.SaveTickerPrice(tickerGroups);
        }

        // Метод добавления тикеров во View (отображение)
        public void AddTickerGroup()
        {
            _tickerPriceStorage = new TickerPriceStorage();

            TickerGroup newGroup = new TickerGroup();

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
                // Запись JSON данных в файл
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось записать данные в файл data.json. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MessageBox.Show("Данные сохранены в JSON файл.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Метод загрузки данных в источник данных для отображения во View (главного окна)
        public void LoadTickerGroups()
        {
            string json = File.ReadAllText("data.json");

            try
            {
                List<TickerGroup> groups = JsonConvert.DeserializeObject<List<TickerGroup>>(json);
                TickerGroups = new ObservableCollection<TickerGroup>(groups);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить данные из файла data.json. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void LoadSavedData()
        {
            TickerPriceStorage tickerPriceStorage = new TickerPriceStorage();

            List<(string GroupName, TickerPriceStorage.TickerPrice Ticker)> savedData = tickerPriceStorage.LoadTickerPrice();
            foreach (var (groupName, ticker) in savedData)
            {
                TrackedTickerInfo info = new TrackedTickerInfo
                {
                    GroupName = groupName,
                    TickerName = ticker.Ticker,
                    Price = ticker.Price,
                    PriceChangePercentage = 0, // Установите нужное значение
                    EventTime = DateTime.Now // Установите нужное значение
                };
                PriceChangeMessages.Add(info);
            }
        }
    }
}

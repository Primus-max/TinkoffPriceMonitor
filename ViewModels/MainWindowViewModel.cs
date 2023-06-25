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




            #region Инициализация источников данных
            // Инициализация источника данных для отображения (настройки)
            TickerGroups = new ObservableCollection<TickerGroup>();

            // Инициализация источника данных для отображения (информация по тикерам)
            PriceChangeMessages = new ObservableCollection<TrackedTickerInfo>();
            #endregion

            #region Вызовы методов            
            LoadTickerGroups();
            Initialize();
            LoadSavedData();
            // AddTickerGroup();
            RunPriceMonitoring();



            #endregion
        }


        public async Task Testing()
        {
            // Создание экземпляра IMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                // Конфигурируйте маппинги здесь
                // Пример: cfg.CreateMap<SourceClass, DestinationClass>();
            });

            IMapper mapper = mapperConfig.CreateMapper();

            foreach (var ticker in TickerGroups)
            {
                string[] tickersSplit = ticker.Tickers.Split('|');

                foreach (var tt in tickersSplit)
                {
                    Share instrument = await GetShareByTicker(tt);
                    // Создание Timestamp для текущего времени
                    Timestamp nowTimestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.Now);

                    // Создание Timestamp для времени, отстоящего от текущего на 5 минут
                    DateTimeOffset fiveMinutesAgo = DateTimeOffset.Now.AddMinutes(-1);
                    Timestamp fiveMinutesAgoTimestamp = Timestamp.FromDateTimeOffset(fiveMinutesAgo);
                    CandleInterval interval = CandleInterval._1Min;

                    var request = new GetCandlesRequest()
                    {
                        InstrumentId = instrument.Uid,
                        From = fiveMinutesAgoTimestamp,
                        To = nowTimestamp,
                        Interval = interval
                    };

                    var response = await _client.MarketData.GetCandlesAsync(request);
                    var result = response.Candles?.Select(x => mapper.Map<Candle>(x)).ToList();

                    // Используйте полученные свечи в дальнейшем анализе
                }
            }
        }


        private void RunPriceMonitoring()
        {
            foreach (var group in TickerGroups)
            {
                Task.Run(() => MonitorTickerGroup(group));
            }
        }

        private async Task MonitorTickerGroup(TickerGroup group)
        {
            string[] tickers = group.Tickers.Split('|');
            TickerPriceStorage tickerPriceStorage = new();

            while (true)
            {
                foreach (var ticker in tickers)
                {
                    // Получаем старую цену
                    decimal oldPrice = tickerPriceStorage.LoadTickerPrice()
                            .SingleOrDefault(t => t.GroupName == group.GroupName && t.Ticker.Ticker == ticker)
                            .Ticker?.Price ?? 0;


                    // Получаем инструмент по тикеру
                    Share instrument = await GetShareByTicker(ticker);

                    if (instrument == null) continue;

                    // Получаем новую цену
                    decimal newPrice = await Getters.GetUpdatedPrice(instrument, _client);

                    // Вычисляем процентное изменение цены
                    decimal priceChangePercentage = (newPrice - oldPrice) / oldPrice * 100;

                    // Обновляем данные в модели
                    //tickerPriceStorage.UpdateTickerPrice(group.GroupName, ticker, newPrice);

                    // При необходимости выполняем дополнительные действия, например, отправку уведомлений

                }

                // Задержка перед следующей проверкой цены
                await Task.Delay(group.Interval);
            }
        }



        private async Task<Share> GetShareByTicker(string ticker)
        {
            SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
            return sharesResponse?.Instruments.FirstOrDefault(x => x.Ticker == ticker);
        }


        #region Методы

        // Метод инициализации клиента и некоторых методов при старте программы
        private async Task Initialize()
        {
            _client = await Creaters.CreateClientAsync();
            await LoadTickerPricesAsync();
            await Testing();
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

        // Метод загрузки и сохранения цен для тикеров из биржи (сохраняем в бинарнике)
        private void LoadSavedData()
        {
            TickerPriceStorage tickerPriceStorage = new();

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
        #endregion
    }
}

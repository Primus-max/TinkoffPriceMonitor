﻿using AutoMapper;
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


        // Метод для запуска мониторинга цен для всех групп тикеров
        private async void RunPriceMonitoring()
        {
            foreach (var group in TickerGroups)
            {
                await MonitorTickerGroup(group);
            }
        }

        // Метод для мониторинга цен для одной группы тикеров
        private async Task MonitorTickerGroup(TickerGroup group)
        {
            string[] tickers = group.Tickers.Split('|');
            TickerPriceStorage tickerPriceStorage = new TickerPriceStorage();

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

                    // Получаем свечи для заданного интервала времени
                    int intervalMinutes = group.Interval;

                    // Преобразование интервала времени в объект TimeSpan
                    TimeSpan timeFrame = TimeSpan.FromMinutes(intervalMinutes);

                    // Получение свечи за заданный интервал времени
                    Candle customCandle = await GetCustomCandle(instrument, timeFrame);

                    // Вычисляем процентное изменение цены
                    decimal priceChangePercentage = CalculatePriceChangePercentage(customCandle);

                    // Проверяем условия и выводим сообщение
                    if (IsPositivePriceChange(customCandle) && priceChangePercentage > group.PercentageThreshold)
                    {
                        // Положительное изменение цены
                        string message = $"Положительное изменение цены для тикера {ticker}: {priceChangePercentage}%";
                        // Отправка сообщения или выполнение дополнительных действий
                    }
                    else if (IsNegativePriceChange(customCandle) && priceChangePercentage > group.PercentageThreshold)
                    {
                        // Отрицательное изменение цены
                        string message = $"Отрицательное изменение цены для тикера {ticker}: {priceChangePercentage}%";
                        // Отправка сообщения или выполнение дополнительных действий
                    }

                    // Обновляем данные в хранилище цен
                    //tickerPriceStorage.UpdateTickerPrice(group.GroupName, ticker, customCandle.Close);
                }

                // Задержка перед следующей проверкой цены
                await Task.Delay(group.Interval);
            }
        }

        // Метод для получения свечей за заданный интервал времени
        private async Task<Candle> GetCustomCandle(Share instrument, TimeSpan timeFrame)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset intervalAgo = now.Subtract(timeFrame);
            Timestamp nowTimestamp = Timestamp.FromDateTimeOffset(now);
            Timestamp intervalAgoTimestamp = Timestamp.FromDateTimeOffset(intervalAgo);

            var request = new GetCandlesRequest()
            {
                InstrumentId = instrument.Uid,
                From = intervalAgoTimestamp,
                To = nowTimestamp,
                Interval = CandleInterval._1Min
            };

            var response = await _client.MarketData.GetCandlesAsync(request);

            // Создание свечи для заданного интервала времени
            Candle customCandle = new Candle
            {
                Open = 0m, // Начальное значение открытия (может быть любым подходящим значением типа decimal)
                Close = 0m, // Начальное значение закрытия (может быть любым подходящим значением типа decimal)
                High = 0m, // Начальное значение максимальной цены (может быть любым подходящим значением типа decimal)
                Low = 0m // Начальное значение минимальной цены (может быть любым подходящим значением типа decimal)
            };

            foreach (var candle in response.Candles)
            {
                // Обновление значений свечи на основе данных из полученных свечей
                customCandle.Open = Math.Min(customCandle.Open, candle.Open);
                customCandle.Close = Math.Max(customCandle.Close, candle.Close);
                customCandle.High = Math.Max(customCandle.High, candle.High);
                customCandle.Low = Math.Min(customCandle.Low, candle.Low);
            }

            return customCandle;
        }

        // Метод для вычисления процентного изменения цены
        private static decimal CalculatePriceChangePercentage(Candle candle)
        {
            decimal open = candle.Open != 0 ? candle.Open : 0.0001m; // Замените 0.0001m на подходящее ненулевое значение
            decimal close = candle.Close != 0 ? candle.Close : 0.0001m; // Замените 0.0001m на подходящее ненулевое значение
            return ((candle.High - candle.Low) * 100) / (open < close ? open : close);
        }

        // Метод для проверки положительного изменения цены
        private static bool IsPositivePriceChange(Candle candle)
        {
            return candle.Open < candle.Close;
        }

        // Метод для проверки отрицательного изменения цены
        private static bool IsNegativePriceChange(Candle candle)
        {
            return candle.Open > candle.Close;
        }





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
            await LoadTickerPricesAsync();

        }

        // Метод загрузки и сохранения цен в барном файле для отображения и дальнейшего сравнения
        private async Task LoadTickerPricesAsync()
        {
            var tickerGroups = new List<TickerPriceStorage.TickerGroup>();

            foreach (var group in TickerGroups)
            {
                if (group is null) continue;
                var tickers = group?.Tickers?.Split('|');

                var tickerGroup = new TickerPriceStorage.TickerGroup
                {
                    GroupName = group?.GroupName,
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

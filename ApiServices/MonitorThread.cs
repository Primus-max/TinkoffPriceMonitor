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
using Candle = Tinkoff.InvestApi.V1.Candle;

namespace TinkoffPriceMonitor.ApiServices
{
    internal class MonitorThread
    {
        private TickerGroup Group { get; }
        private InvestApiClient? _client;
        public event Action<TrackedTickerInfo> PriceChangeSignal;

        public MonitorThread(TickerGroup group, InvestApiClient? client)
        {
            Group = group;
            _client = client;
        }

        public MonitorThread()
        {

        }

        public async Task StartMonitoringAsync()
        {
            string[] tickers = Group.Tickers.Split('|');

            while (true)
            {
                //PriceChangeMessages.Clear();

                foreach (var ticker in tickers)
                {
                    Share instrument = await GetShareByTicker(ticker);

                    if (instrument == null) continue;

                    int intervalMinutes = Group.Interval;
                    TimeSpan timeFrame = TimeSpan.FromMinutes(intervalMinutes);
                    Candle customCandle = await GetCustomCandle(instrument, timeFrame);

                    if (customCandle is null) continue;

                    CalculateAndDisplayPriceChange(customCandle, Group.PercentageThreshold, ticker);
                }

                // Задержка перед следующей проверкой цены для данной группы
                await Task.Delay(Group.Interval);
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

            try
            {
                var response = await _client?.MarketData.GetCandlesAsync(request);

                if (response?.Candles is null || response.Candles.Count == 0)
                {
                    return new Candle();
                }

                // Создание свечи для заданного интервала времени
                Candle customCandle = new Candle
                {
                    Open = decimal.MaxValue,
                    Close = decimal.MinValue,
                    High = decimal.MinValue,
                    Low = decimal.MaxValue
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении свечей. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Candle();
            }
        }

        // Метод для вычисления процентного изменения цены
        private void CalculateAndDisplayPriceChange(Candle customCandle, decimal p, string ticker)
        {
            decimal open = customCandle.Open != null && customCandle.Open != 0 ? customCandle.Open : 0.0001m;
            decimal close = customCandle.Close != null && customCandle.Close != 0 ? customCandle.Close : 0.0001m;
            decimal high = customCandle.High;
            decimal low = customCandle.Low;

            decimal priceChangePercentage = 0;

            // Проверяем, что значение low не равно нулю перед делением
            if (low != 0)
            {
                priceChangePercentage = ((high - low) * 100 / low);
            }

            decimal roundedPercentage = Math.Round(priceChangePercentage, 2);
            if (open < close)
            {
                // Положительное изменение цены
                if (priceChangePercentage > p)
                {
                    // Вызываем событие для передачи информации в MainWindowViewModel

                    TrackedTickerInfo trackedTickerInfo = new();
                    trackedTickerInfo.IsPositivePriceChange = true;
                    trackedTickerInfo.PriceChangePercentage = roundedPercentage;
                    trackedTickerInfo.GroupName = Group.GroupName;
                    trackedTickerInfo.TickerName = ticker;
                    trackedTickerInfo.EventTime = DateTime.Now;

                    PriceChangeSignal?.Invoke(trackedTickerInfo);

                    //PriceChangeMessages.Add(trackedTickerInfo);
                    // Ваш код для передачи информации во View или выполнения других действий
                    string message1 = $"Сигнал: Цена поднялась на {priceChangePercentage:F2}%.";
                }
            }
            else if (open > close)
            {
                TrackedTickerInfo trackedTickerInfo = new();
                trackedTickerInfo.IsPositivePriceChange = false;
                trackedTickerInfo.PriceChangePercentage = roundedPercentage;
                trackedTickerInfo.GroupName = Group.GroupName;
                trackedTickerInfo.TickerName = ticker;
                trackedTickerInfo.EventTime = DateTime.Now;

                PriceChangeSignal?.Invoke(trackedTickerInfo);
                string message2 = $"Сигнал: Цена поднялась на {priceChangePercentage:F2}%.";
            }
        }

        private async Task<Share> GetShareByTicker(string ticker)
        {
            Share share = new();
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

        public class Candle
        {
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
        }
    }
}

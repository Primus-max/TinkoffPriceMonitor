using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using TinkoffPriceMonitor.Models;

namespace TinkoffPriceMonitor.ApiServices
{
    internal class MonitorThread
    {
        private TickerGroup Group { get; }
        private InvestApiClient? _client = null!;

        public event Action<TrackedTickerInfo> PriceChangeSignal = null!;


        public MonitorThread(TickerGroup group, InvestApiClient? client)
        {
            Group = group;
            _client = client;
        }

        public async Task StartMonitoringAsync()
        {
            string[] tickers = Group.Tickers.Split('|');


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
            // await Task.Delay(Group.Interval);

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

            if (low > 0)
            {
                if (open < close)
                {
                    priceChangePercentage = ((high - low) * 100 / low);
                }
                else if (open > close)
                {
                    priceChangePercentage = ((high - low) * 100 / high);
                }
            }

            decimal roundedPercentage = Math.Round(priceChangePercentage, 2);

            if (open < close && priceChangePercentage > p)
            {
                TrackedTickerInfo trackedTickerInfo = new TrackedTickerInfo();
                trackedTickerInfo.IsPositivePriceChange = true;
                trackedTickerInfo.PriceChangePercentage = roundedPercentage;
                trackedTickerInfo.GroupName = Group.GroupName;
                trackedTickerInfo.TickerName = ticker;
                trackedTickerInfo.EventTime = DateTime.Now;

                PriceChangeSignal?.Invoke(trackedTickerInfo);
                string message = $"Сигнал: Цена поднялась на {roundedPercentage:F2}%.";
                // Дальнейшая логика или вывод сообщения
            }
            if (open > close && priceChangePercentage > p)
            {
                TrackedTickerInfo trackedTickerInfo = new TrackedTickerInfo();
                trackedTickerInfo.IsPositivePriceChange = false;
                trackedTickerInfo.PriceChangePercentage = -roundedPercentage; // Изменяем знак на отрицательный
                trackedTickerInfo.GroupName = Group.GroupName;
                trackedTickerInfo.TickerName = ticker;
                trackedTickerInfo.EventTime = DateTime.Now;

                PriceChangeSignal?.Invoke(trackedTickerInfo);

                string message = $"Сигнал: Цена снизилась на {Math.Abs(roundedPercentage):F2}%.";
                // Дальнейшая логика или вывод сообщения
            }

            else
            {
                string message = $"Сигнал: Цена изменилась, но не достаточно значительно.";
                // Дальнейшая логика или вывод сообщения
            }
        }

        // Метод получения инструмента по тикеру
        private async Task<Share> GetShareByTicker(string ticker)
        {
            Share share = new();
            try
            {
                // Получаю инструмент 
                SharesResponse sharesResponse = await _client?.Instruments.SharesAsync();
                share = sharesResponse?.Instruments?.FirstOrDefault(x => x.Ticker == ticker) ?? new Share();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось получить инструмент. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return share;
        }

        // Мотдель свечи
        public class Candle
        {
            public decimal Open { get; set; }
            public decimal Close { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
        }
    }
}

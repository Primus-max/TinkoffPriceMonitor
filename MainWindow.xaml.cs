using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.InvestApi;
using Grpc.Net.ClientFactory;

namespace TinkoffPriceMonitor
{
    //public partial class MainWindow : Window
    //{
    //    private readonly TinkoffInvestApiWrapper apiWrapper;
    //    private readonly Dictionary<string, CandlePayload> lastCandles = new Dictionary<string, CandlePayload>();
    //    private readonly object candlesLock = new object();
    //    private bool alwaysOnTopEnabled = false;

    //    private List<string> tickerGroup1;
    //    private double percentageThresholdGroup1;
    //    private TimeSpan intervalGroup1;

    //    private List<string> tickerGroup2;
    //    private double percentageThresholdGroup2;
    //    private TimeSpan intervalGroup2;

    //    private List<string> tickerGroup3;
    //    private double percentageThresholdGroup3;
    //    private TimeSpan intervalGroup3;

    //    private int widgetGroupNumber;
    //    private decimal lotSizeAmount;

    //    public MainWindow()
    //    {
    //        InitializeComponent();
    //        apiWrapper = new TinkoffInvestApiWrapper("YOUR_API_TOKEN");
    //        LoadSettingsFromFile("settings.txt");
    //        StartMonitoring();                       
    //    }

    //    private void LoadSettingsFromFile(string filePath)
    //    {
    //        string[] settings = File.ReadAllLines(filePath);

    //        tickerGroup1 = settings[1].Split('|').Select(ticker => ticker.Trim()).ToList();
    //        percentageThresholdGroup1 = double.Parse(settings[2]);
    //        intervalGroup1 = TimeSpan.Parse(settings[3]);

    //        tickerGroup2 = settings[5].Split('|').Select(ticker => ticker.Trim()).ToList();
    //        percentageThresholdGroup2 = double.Parse(settings[6]);
    //        intervalGroup2 = TimeSpan.Parse(settings[7]);

    //        tickerGroup3 = settings[9].Split('|').Select(ticker => ticker.Trim()).ToList();
    //        percentageThresholdGroup3 = double.Parse(settings[10]);
    //        intervalGroup3 = TimeSpan.Parse(settings[11]);

    //        widgetGroupNumber = int.Parse(settings[13]);
    //        lotSizeAmount = decimal.Parse(settings[14]);
    //    }

    //    private void StartMonitoring()
    //    {
    //        Thread monitoringThread = new Thread(() =>
    //        {
    //            while (true)
    //            {
    //                MonitorPriceChangesForGroup(tickerGroup1, percentageThresholdGroup1, intervalGroup1);
    //                MonitorPriceChangesForGroup(tickerGroup2, percentageThresholdGroup2, intervalGroup2);
    //                MonitorPriceChangesForGroup(tickerGroup3, percentageThresholdGroup3, intervalGroup3);

    //                Thread.Sleep(5000); // Sleep for 5 seconds before checking again
    //            }
    //        });

    //        monitoringThread.IsBackground = true;
    //        monitoringThread.Start();
    //    }

    //    private void MonitorPriceChangesForGroup(List<string> tickers, double percentageThreshold, TimeSpan interval)
    //    {
    //        foreach (string ticker in tickers)
    //        {
    //            CandlePayload currentCandle = GetLatestCandle(ticker);
    //            if (currentCandle != null)
    //            {
    //                CandlePayload previousCandle = GetPreviousCandle(ticker);
    //                if (previousCandle != null)
    //                {
    //                    double priceChangePercentage = CalculatePriceChangePercentage(previousCandle, currentCandle);
    //                    if (priceChangePercentage >= percentageThreshold)
    //                    {
    //                        string message = GetPriceChangeMessage(ticker, priceChangePercentage);
    //                        ShowMessage(message);
    //                        Thread.Sleep(interval); // Sleep for the specified interval before showing another message for the same ticker
    //                    }
    //                }

    //                lock (candlesLock)
    //                {
    //                    lastCandles[ticker] = currentCandle;
    //                }
    //            }
    //        }
    //    }

    //    //private CandlePayload GetLatestCandle(string ticker)
    //    //{
    //    //    return TinkoffInvestApiWrapper.GetLatestCandle(ticker, CandleInterval.Minute);
    //    //}

    //    private CandlePayload GetPreviousCandle(string ticker)
    //    {
    //        lock (candlesLock)
    //        {
    //            if (lastCandles.ContainsKey(ticker))
    //            {
    //                return lastCandles[ticker];
    //            }
    //        }

    //        return null;
    //    }

    //    //private double CalculatePriceChangePercentage(CandlePayload previousCandle, CandlePayload currentCandle)
    //    //{
    //    //    double priceChangePercentage = 0;

    //    //    if (currentCandle.Open > currentCandle.Close)
    //    //    {
    //    //        double priceRange = (double)(currentCandle.High - currentCandle.Low);
    //    //        priceChangePercentage = (priceRange * 100) / currentCandle.Low;
    //    //    }
    //    //    else if (currentCandle.Open < currentCandle.Close)
    //    //    {
    //    //        double priceRange = (double)(currentCandle.High - currentCandle.Low);
    //    //        priceChangePercentage = (priceRange * 100) / currentCandle.High;
    //    //    }

    //    //    return Math.Round(priceChangePercentage, 1);
    //    //}

    //    private string GetPriceChangeMessage(string ticker, double priceChangePercentage)
    //    {
    //        string direction = priceChangePercentage >= 0 ? "положительное" : "отрицательное";
    //        return $"Изменение цены для тикера {ticker}: {direction}, процент: {Math.Abs(priceChangePercentage)}%";
    //    }

    //    private void ShowMessage(string message)
    //    {
    //        Dispatcher.Invoke(() =>
    //        {
    //            MessageBox.Show(message, "Цена изменилась");
    //        });
    //    }

    //    private void alwaysOnTopCheckBox_Checked(object sender, RoutedEventArgs e)
    //    {
    //        alwaysOnTopEnabled = true;
    //        Topmost = true;
    //    }

    //    private void alwaysOnTopCheckBox_Unchecked(object sender, RoutedEventArgs e)
    //    {
    //        alwaysOnTopEnabled = false;
    //        Topmost = false;
    //    }

    //    private void goToTerminalButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        // Code to navigate to the Tinkoff web terminal with the specified widget group and lot size amount
    //        // ...
    //    }

    //    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    //    {
    //        DragMove();
    //    }

    //    private void exitMenuItem_Click(object sender, RoutedEventArgs e)
    //    {
    //        SaveSettingsToFile("settings.txt");
    //        Application.Current.Shutdown();
    //    }

    //    private void SaveSettingsToFile(string filePath)
    //    {
    //        List<string> settings = new List<string>
    //        {
    //            "Токен API тинькофф",
    //            string.Join(" | ", tickerGroup1),
    //            percentageThresholdGroup1.ToString(),
    //            intervalGroup1.ToString(),
    //            string.Join(" | ", tickerGroup2),
    //            percentageThresholdGroup2.ToString(),
    //            intervalGroup2.ToString(),
    //            string.Join(" | ", tickerGroup3),
    //            percentageThresholdGroup3.ToString(),
    //            intervalGroup3.ToString(),
    //            widgetGroupNumber.ToString(),
    //            lotSizeAmount.ToString()
    //        };

    //        File.WriteAllLines(filePath, settings);
    //    }
    //}

    public class TinkoffInvestApiWrapper
    {
        private readonly string apiToken;

        public TinkoffInvestApiWrapper(string apiToken)
        {
            this.apiToken = apiToken;
        }

        //public static CandlePayload GetLatestCandle(string ticker, CandleInterval interval)
        //{
        //    // Use Tinkoff API to retrieve the latest candle for the specified ticker and interval
        //    // ...
        //    CandlePayload candle ;
        //    return candle;
        //}
    }
}

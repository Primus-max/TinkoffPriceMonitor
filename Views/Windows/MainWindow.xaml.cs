using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using TinkoffPriceMonitor.Models;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }


        public void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Получение данных из полей и создание списка TickerGroup
            var tickerGroups = new List<TickerGroup>();

            // Получение данных для первой группы тикеров
            var tickerGroup1 = new TickerGroup
            {
                Tickers = ParseTickers(TickerGroup1.Text),
                PercentageThreshold = ParsePercentageThreshold(PercentageThresholdGroup1.Text),
                Interval = ParseInterval(IntervalGroup1.Text)
            };
            tickerGroups.Add(tickerGroup1);

            // Получение данных для второй группы тикеров
            var tickerGroup2 = new TickerGroup
            {
                Tickers = ParseTickers(TickerGroup2.Text),
                PercentageThreshold = ParsePercentageThreshold(PercentageThresholdGroup2.Text),
                Interval = ParseInterval(IntervalGroup2.Text)
            };
            tickerGroups.Add(tickerGroup2);

            // Получение данных для третьей группы тикеров
            var tickerGroup3 = new TickerGroup
            {
                Tickers = ParseTickers(TickerGroup3.Text),
                PercentageThreshold = ParsePercentageThreshold(PercentageThresholdGroup3.Text),
                Interval = ParseInterval(IntervalGroup3.Text)
            };
            tickerGroups.Add(tickerGroup3);

            // Создание объекта AppSettings и заполнение свойства TickerGroups
            var settings = new AppSettings
            {
                TickerGroups = tickerGroups
            };

            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText("appSettings.json", json);

                MessageBox.Show("Настройки сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Обработка ошибки сохранения настроек
            }
        }

        private List<string> ParseTickers(string tickerGroup)
        {
            // Разделение строки на тикеры и удаление пустых элементов
            return tickerGroup.Split('|').Where(ticker => !string.IsNullOrWhiteSpace(ticker)).ToList();
        }

        private double ParsePercentageThreshold(string percentageThreshold)
        {
            double result;
            if (!double.TryParse(percentageThreshold, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                // Обработка ошибки преобразования процентного порога
                result = 0.0; // Установка значения по умолчанию
            }
            return result;
        }

        private int ParseInterval(string interval)
        {
            int result;
            if (!int.TryParse(interval, out result))
            {
                // Обработка ошибки преобразования интервала
                result = 0; // Установка значения по умолчанию
            }
            return result;
        }

        private void LoadSettings()
        {
            try
            {
                string json = File.ReadAllText("appSettings.json");
                AppSettings settings = JsonConvert.DeserializeObject<AppSettings>(json);

                TickerGroup1.Text = settings.TickerGroup1 ?? string.Empty;
                PercentageThresholdGroup1.Text = settings.PercentageThresholdGroup1.ToString(CultureInfo.InvariantCulture);
                IntervalGroup1.Text = settings.IntervalGroup1.ToString();

                TickerGroup2.Text = settings.TickerGroup2 ?? string.Empty;
                PercentageThresholdGroup2.Text = settings.PercentageThresholdGroup2.ToString(CultureInfo.InvariantCulture);
                IntervalGroup2.Text = settings.IntervalGroup2.ToString();

                TickerGroup3.Text = settings.TickerGroup3 ?? string.Empty;
                PercentageThresholdGroup3.Text = settings.PercentageThresholdGroup3.ToString(CultureInfo.InvariantCulture);
                IntervalGroup3.Text = settings.IntervalGroup3.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить настройки {ex.Message}");
            }

        }

    }
}

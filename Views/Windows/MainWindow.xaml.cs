using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
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

        public void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            #region Получение данных из полей
            string tickerGroup1 = TickerGroup1.Text;
            double percentageThresholdGroup1;
            int intervalGroup1;

            if (!double.TryParse(PercentageThresholdGroup1.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out percentageThresholdGroup1))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                percentageThresholdGroup1 = 0.0; // Например, установка значения по умолчанию
            }

            if (!int.TryParse(IntervalGroup1.Text, out intervalGroup1))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                intervalGroup1 = 0; // Например, установка значения по умолчанию
            }

            string tickerGroup2 = TickerGroup2.Text;
            double percentageThresholdGroup2;
            int intervalGroup2;

            if (!double.TryParse(PercentageThresholdGroup2.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out percentageThresholdGroup2))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                percentageThresholdGroup2 = 0.0; // Например, установка значения по умолчанию
            }

            if (!int.TryParse(IntervalGroup2.Text, out intervalGroup2))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                intervalGroup2 = 0; // Например, установка значения по умолчанию
            }

            string tickerGroup3 = TickerGroup3.Text;
            double percentageThresholdGroup3;
            int intervalGroup3;

            if (!double.TryParse(PercentageThresholdGroup3.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out percentageThresholdGroup3))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                percentageThresholdGroup3 = 0.0; // Например, установка значения по умолчанию
            }

            if (!int.TryParse(IntervalGroup3.Text, out intervalGroup3))
            {
                // Обработка ошибки, если не удалось преобразовать строку в число
                // Установка значения по умолчанию или другое действие
                intervalGroup3 = 0; // Например, установка значения по умолчанию
            }


            #endregion


            // Запись настроек в файл JSON с использованием Newtonsoft.Json
            var settings = new AppSettings
            {
                TickerGroup1 = tickerGroup1,
                PercentageThresholdGroup1 = percentageThresholdGroup1,
                IntervalGroup1 = intervalGroup1,
                TickerGroup2 = tickerGroup2,
                PercentageThresholdGroup2 = percentageThresholdGroup2,
                IntervalGroup2 = intervalGroup2,
                TickerGroup3 = tickerGroup3,
                PercentageThresholdGroup3 = percentageThresholdGroup3,
                IntervalGroup3 = intervalGroup3
            };


            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText("appSettings.json", json);
            }
            catch (Exception ex)
            {

            }
        }

    }
}

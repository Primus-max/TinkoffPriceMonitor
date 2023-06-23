using Newtonsoft.Json;
using System;
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
            // Получите значения полей ввода из свойств вашей модели представления
            string tickerGroup1 = TickerGroup1.Text;
            double percentageThresholdGroup1 = Convert.ToDouble(PercentageThresholdGroup1.Text);
            int intervalGroup1 = Convert.ToInt32(IntervalGroup1.Text);

            string tickerGroup2 = TickerGroup2.Text;
            double percentageThresholdGroup2 = Convert.ToDouble(PercentageThresholdGroup2.Text);
            int intervalGroup2 = Convert.ToInt32(IntervalGroup2.Text);

            string tickerGroup3 = TickerGroup3.Text;
            double percentageThresholdGroup3 = Convert.ToDouble(PercentageThresholdGroup3.Text);
            int intervalGroup3 = Convert.ToInt32(IntervalGroup3.Text);


            // Здесь выполните логику сохранения настроек в файл appSettings.json
            // Используйте полученные значения для записи в файл

            // Пример записи настроек в файл JSON с использованием Newtonsoft.Json
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

            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText("appSettings.json", json);
        }

    }
}

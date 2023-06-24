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
        private ObservableCollection<PriceChangeMessage> _priceChangeMessages = null;
        #endregion

        #region Публичные свойства
        public ObservableCollection<PriceChangeMessage> PriceChangeMessages
        {
            get => _priceChangeMessages;
            set => Set(ref _priceChangeMessages, value);
        }
        #endregion

        private ObservableCollection<TickerGroup> _tickerGroups;

        public ObservableCollection<TickerGroup> TickerGroups
        {
            get => _tickerGroups;
            set => Set(ref _tickerGroups, value);
        }

        public MainWindowViewModel()
        {
            TickerGroups = new ObservableCollection<TickerGroup>();
            LoadTickerGroups();
        }

        public void AddTickerGroup()
        {
            TickerGroup newGroup = new TickerGroup
            {
                GroupName = "Group 1",
                Tickers = new List<string>
                {
                    "AAPL",
                    "GOOGL",
                    "MSFT"
                },
                PercentageThreshold = 0.05,
                Interval = 60
            };


            TickerGroups.Add(newGroup);
        }

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



    }
}

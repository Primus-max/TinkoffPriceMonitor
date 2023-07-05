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
        // private TickerPriceStorage _tickerPriceStorage;
        private ObservableCollection<TickerGroup> _tickerGroups;
        private bool _IsPositivePriceChange = false;
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

        public bool IsPositivePriceChange
        {
            get => _IsPositivePriceChange;
            set => Set(ref _IsPositivePriceChange, value);
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

            #region Подписка на события

            #endregion

            #region Вызовы методов            
            LoadTickerGroups();
            Initialize();
            //LoadSavedData();
            // AddTickerGroup();
            RunPriceMonitoring();
            #endregion
        }


        // Метод для запуска мониторинга цен для всех групп тикеров
        //private async void RunPriceMonitoring()
        //{
        //    foreach (var group in TickerGroups)
        //    {
        //        MonitorThread monitor = new(group, _client);
        //        monitor.PriceChangeSignal += MonitorThread_PriceChangeSignal;

        //        await monitor.StartMonitoringAsync();

        //        //Thread.Sleep(10000);
        //        //await MonitorTickerGroup(group);

        //    }
        //}

        private async Task RunPriceMonitoring()
        {
            List<Task> monitorTasks = new List<Task>();

            foreach (var group in TickerGroups)
            {
                var monitorTask = Task.Run(async () =>
                {
                    while (true)
                    {
                        MonitorThread monitor = new MonitorThread(group, _client);
                        monitor.PriceChangeSignal += MonitorThread_PriceChangeSignal;
                        await monitor.StartMonitoringAsync();

                        int delayMilliseconds = group.Interval * 60 * 1000; // Преобразование минут в миллисекунды
                        await Task.Delay(delayMilliseconds);
                    }
                });

                monitorTasks.Add(monitorTask);
            }

            await Task.WhenAll(monitorTasks);
        }


        #region Подписчики на события
        private void MonitorThread_PriceChangeSignal(TrackedTickerInfo trackedTickerInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Проверяем, существует ли элемент с таким же именем тикера в коллекции
                var existingItem = PriceChangeMessages.FirstOrDefault(item => item.TickerName == trackedTickerInfo.TickerName);

                if (existingItem != null)
                {
                    // Обновляем существующий элемент новыми данными
                    existingItem.IsPositivePriceChange = trackedTickerInfo.IsPositivePriceChange;
                    existingItem.PriceChangePercentage = trackedTickerInfo.PriceChangePercentage;
                    existingItem.GroupName = trackedTickerInfo.GroupName;
                    existingItem.EventTime = DateTime.Now;
                }
                else
                {
                    // Добавляем новый элемент в коллекцию
                    PriceChangeMessages.Add(trackedTickerInfo);
                }
            });
        }

        #endregion

        #region Методы
        // Метод инициализации клиента и некоторых методов при старте программы
        private async Task Initialize()
        {
            _client = await Creaters.CreateClientAsync();
            //await LoadTickerPricesAsync();
        }

        // Метод добавления группы тикеров во View (отображение)
        public void AddTickerGroup()
        {
            //_tickerPriceStorage = new TickerPriceStorage();

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
        #endregion
    }
}

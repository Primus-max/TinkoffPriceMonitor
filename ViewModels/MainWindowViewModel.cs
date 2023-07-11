using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tinkoff.InvestApi;
using TinkoffPriceMonitor.ApiServices;
using TinkoffPriceMonitor.ApiServices.ChromeAPIExtensions;
using TinkoffPriceMonitor.Logs;
using TinkoffPriceMonitor.Models;
using TinkoffPriceMonitor.ViewModels.BaseView;

namespace TinkoffPriceMonitor.ViewModels
{

public class MainWindowViewModel : BaseViewModel
{
#region Приватные свойства
private InvestApiClient? _client = null;
private ObservableCollection<TrackedTickerInfo> _priceChangeMessages = null!;
    private ObservableCollection<TickerGroup> _tickerGroups = null!;
        private bool _IsPositivePriceChange = false;
        private SettingsModel _settingsModel = null!;
        private TrackedTickerInfo _selectedTickerGroup = null!;
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

                                        public SettingsModel SettingsModel
                                        {
get => _settingsModel;
set => Set(ref _settingsModel, value);
                                                }

                                                public TrackedTickerInfo SelectedTickerGroup
                                                {
get => _selectedTickerGroup;
set => Set(ref _selectedTickerGroup, value);
                                                        }
                                                        #endregion

                                                        #region комманды
                                                        public ICommand SaveCommand { get; private set; }

                                                        public ICommand RunTerminalCommand { get; private set; }

                                                        #endregion

                                                        public MainWindowViewModel()
                                                        {
                                                        // тестовое подключение
                                                        //TinkoffTerminalManager terminalmanager = new TinkoffTerminalManager();
                                                        //terminalmanager.Start("");



                                                        //terminalmanager.Close();



                                                        #region Инициализация источников данных

                                                        LoggerInitializer.InitializeLogger();
                                                        // Инициализация источника данных для токена и адреса хрома
                                                        SettingsModel = new SettingsModel();

                                                        // Инициализация источника данных для отображения (настройки)
                                                        TickerGroups = new ObservableCollection<TickerGroup>();

                                                            // Инициализация источника данных для отображения (информация по тикерам)
                                                            PriceChangeMessages = new ObservableCollection<TrackedTickerInfo>();

                                                                // Инициализация выбранного элемента
                                                                SelectedTickerGroup = new TrackedTickerInfo();

                                                                // Инициализация комманд
                                                                SaveCommand = new RelayCommand(SaveSettings);
                                                                RunTerminalCommand = new RelayCommand(RunTinkoffMonitor);
                                                                #endregion

                                                                #region Подписка на события

                                                                #endregion

                                                                #region Вызовы методов
                                                                LoadSettings();
                                                                LoadTickerGroups();
                                                                Initialize();
                                                                //LoadSavedData();
                                                                // AddTickerGroup();
                                                                RunPriceMonitoring();
                                                                #endregion
                                                                }


                                                                // Метод мониторинга тикеров
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

                                                                                    // Сортировка коллекции по имени группы, и обновление для view
                                                                                    PriceChangeMessages = new ObservableCollection<TrackedTickerInfo>(PriceChangeMessages
.OrderBy(item => item.GroupName));
                                                                                            //UpdateItemsForGroup(existingItem.GroupName);
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                            // Добавляем новый элемент в коллекцию
                                                                                            PriceChangeMessages.Add(trackedTickerInfo);
                                                                                            }
                                                                                            });
                                                                                            }

                                                                                            #endregion

                                                                                            private void UpdateItemsForGroup(string groupName)
                                                                                            {
                                                                                            // Создаем новую коллекцию с обновленными элементами
                                                                                            ObservableCollection<TrackedTickerInfo> updatedItems = new ObservableCollection<TrackedTickerInfo>();

                                                                                                    foreach (var item in PriceChangeMessages)
                                                                                                    {
                                                                                                    if (item.GroupName == groupName)
                                                                                                    {
                                                                                                    // Клонируем элемент, чтобы создать новый экземпляр с обновленными значениями свойств
                                                                                                    TrackedTickerInfo updatedItem = new TrackedTickerInfo()
                                                                                                    {
                                                                                                    IsPositivePriceChange = item.IsPositivePriceChange,
                                                                                                    PriceChangePercentage = item.PriceChangePercentage,
                                                                                                    GroupName = item.GroupName,
                                                                                                    TickerName = item.TickerName, // Обновляем имя тикера
                                                                                                    EventTime = DateTime.Now // Обновляем время только для элементов текущей группы
                                                                                                    };

                                                                                                    // Добавляем обновленный элемент в новую коллекцию
                                                                                                    updatedItems.Add(updatedItem);
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                    // Для элементов других групп просто добавляем в новую коллекцию без изменений
                                                                                                    updatedItems.Add(item);
                                                                                                    }
                                                                                                    }

                                                                                                    // Заменяем коллекцию PriceChangeMessages на новую коллекцию с обновленными элементами
                                                                                                    PriceChangeMessages = updatedItems;
                                                                                                    }


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
                                                                                                    string? jsonData = JsonConvert.SerializeObject(TickerGroups);

                                                                                                    // Получение пути к файлу в корне программы
                                                                                                    string? filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");

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
                                                                                                    string? filePath = "data.json";

                                                                                                    if (File.Exists(filePath))
                                                                                                    {
                                                                                                    string? json = File.ReadAllText(filePath);

                                                                                                    try
                                                                                                    {
                                                                                                    List<TickerGroup> groups = JsonConvert.DeserializeObject<List<TickerGroup>>(json);
                                                                                                                TickerGroups = new ObservableCollection<TickerGroup>(groups);
                                                                                                                    }
                                                                                                                    catch (Exception ex)
                                                                                                                    {
                                                                                                                    Log.Error($"Не удалось загрузить данные из файла data.json: {ex.Message}");
                                                                                                                    }
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                    //MessageBox.Show($"Файл {filePath} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                                                                                                    }
                                                                                                                    }

                                                                                                                    // Сохранение пути к Chrome и токен
                                                                                                                    private void SaveSettings()
                                                                                                                    {
                                                                                                                    // Получите значения из свойств вашей модели представления (SettingsModel)
                                                                                                                    string? tinkoffToken = SettingsModel.TinkoffToken;
                                                                                                                    string? chromeLocation = SettingsModel.ChromeLocation;

                                                                                                                    // Выполните сохранение данных в JSON

                                                                                                                    // Пример сохранения данных в JSON
                                                                                                                    JObject data = new JObject();
                                                                                                                    data["TinkoffToken"] = tinkoffToken;
                                                                                                                    data["ChromeLocation"] = chromeLocation;
                                                                                                                    data["PinCode"] = chromeLocation;

                                                                                                                    string? jsonData = data.ToString();
                                                                                                                    string? filePath = "settings.json";

                                                                                                                    try
                                                                                                                    {
                                                                                                                    if (!File.Exists(filePath))
                                                                                                                    {
                                                                                                                    // Если файл не существует, создайте новый файл
                                                                                                                    using StreamWriter file = File.CreateText(filePath);
                                                                                                                    file.Write(jsonData);

                                                                                                                    MessageBox.Show("Данные успешно сохранены!");
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                    // Если файл существует, перезапишите его содержимое
                                                                                                                    File.WriteAllText(filePath, jsonData);
                                                                                                                    MessageBox.Show("Данные успешно сохранены!");
                                                                                                                    }
                                                                                                                    }
                                                                                                                    catch (Exception ex)
                                                                                                                    {
                                                                                                                    MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}");
                                                                                                                    }
                                                                                                                    }

                                                                                                                    // Загружаю данные для отображения, токен и путь к хром, при инициализации
                                                                                                                    private void LoadSettings()
                                                                                                                    {
                                                                                                                    string? filePath = "settings.json";

                                                                                                                    try
                                                                                                                    {
                                                                                                                    if (File.Exists(filePath))
                                                                                                                    {
                                                                                                                    // Если файл существует, загрузите его содержимое
                                                                                                                    string? jsonData = File.ReadAllText(filePath);
                                                                                                                    JObject data = JObject.Parse(jsonData);

                                                                                                                    // Пример загрузки данных из JSON в модель представления
                                                                                                                    SettingsModel.TinkoffToken = data["TinkoffToken"]?.ToString();
                                                                                                                    SettingsModel.ChromeLocation = data["ChromeLocation"]?.ToString();
                                                                                                                    SettingsModel.PinCode = data["PinCode"]?.ToString();
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                    // Если файла нет, создайте новую модель представления
                                                                                                                    SettingsModel = new SettingsModel();
                                                                                                                    }
                                                                                                                    }
                                                                                                                    catch (Exception ex)
                                                                                                                    {
                                                                                                                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
                                                                                                                    }
                                                                                                                    }

                                                                                                                    // Метод вызовы терминала и вставка данных в поля
                                                                                                                    public void RunTinkoffMonitor()
                                                                                                                    {

                                                                                                                    // Получение данных из выбранного тикера
                                                                                                                    string? selectedTickerGroupName = SelectedTickerGroup.GroupName;

                                                                                                                    // Поиск элемента по имени группы
var selectedTicker = TickerGroups.FirstOrDefault(t => t.GroupName == selectedTickerGroupName);

                                                                                                                        if (selectedTicker != null)
                                                                                                                        {
                                                                                                                        string? tickerGroupName = selectedTicker.WidgetGroupNumber.ToString();
                                                                                                                        string? orderAmount = selectedTicker.OrderAmountRubles.ToString();

                                                                                                                        TinkoffTerminalManager terminalManager = new TinkoffTerminalManager(tickerGroupName, orderAmount);

                                                                                                                        terminalManager.Start();
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                        // Элемент не найден
                                                                                                                        }
                                                                                                                        }

                                                                                                                        #endregion
                                                                                                                        }
                                                                                                                        }

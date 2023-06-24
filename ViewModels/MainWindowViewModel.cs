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



        public MainWindowViewModel()
        {

            InitializeClientAsync();


            AppSettings instruments = LoadAppSettingsFromFile();

        }

        // Получаю клиента для работы
        private async void InitializeClientAsync()
        {
            _client = await Creaters.CreateClientAsync();

            AppSettings instruments = LoadAppSettingsFromFile();

            await FetchPriceChangeMessages();
        }

        private async Task FetchPriceChangeMessages()
        {
            #region ТЕСТЫ ЗАПИСИ И ЧТЕНИЯ В БИНАРНИК
            // Создаем экземпляр хранилища
            TickerPriceStorage storage = new TickerPriceStorage();

            // Создаем проверочные данные
            List<TickerGroup> tickerGroups = new List<TickerGroup>();

            TickerGroup group1 = new TickerGroup()
            {
                GroupName = "Group1",
                Tickers = new List<TickerPrice>()
            {
                new TickerPrice() { Ticker = "GOOG", Price = 2500.75m },
                new TickerPrice() { Ticker = "MSFT", Price = 300.50m },
                new TickerPrice() { Ticker = "AAPL", Price = 150.25m }
            }
            };

            TickerGroup group2 = new TickerGroup()
            {
                GroupName = "Group2",
                Tickers = new List<TickerPrice>()
            {
                new TickerPrice() { Ticker = "AMZN", Price = 3500.00m },
                new TickerPrice() { Ticker = "FB", Price = 350.50m }
            }
            };

            tickerGroups.Add(group1);
            tickerGroups.Add(group2);

            // Сохраняем данные
            storage.SaveTickerPrice(tickerGroups);

            // Загружаем данные
            List<TickerGroup> loadedTickerGroups = storage.LoadTickerPrice();

            // Выводим загруженные данные
            foreach (TickerGroup group in loadedTickerGroups)
            {
                MessageBox.Show("Group Name: " + group.GroupName);
                MessageBox.Show("Tickers:");

                foreach (TickerPrice ticker in group.Tickers)
                {
                    MessageBox.Show("Ticker: " + ticker.Ticker);
                    MessageBox.Show("Price: " + ticker.Price);
                }
            }
            #endregion



            var instruments = new List<Instrument>();
            instruments.Add(new Instrument() { Figi = "BBG004S68B31" });
            instruments.Add(new Instrument() { Figi = "BBG000TY1CD1" });

            var nstruments = await _client?.Instruments?.SharesAsync();
            Instrument instrument1 = new Instrument();


            foreach (var instrument in nstruments.Instruments)
            {
                var adfasdf = instrument.Ticker;

                if (adfasdf == "BELU")
                {
                    string figi1 = instrument.Figi;
                }
                if (adfasdf == "ALRS")
                {
                    string figi1 = instrument.Figi;
                }
            }

            // Вызываем метод GetPriceChangeMessages и передаем список инструментов и клиента API
            var priceChangeMessages = await Getters.GetUpdatedPrices(instruments, _client);

            // Далее вы можете обработать полученные priceChangeMessages в соответствии с вашими требованиями
            // Например, можно отобразить их на экране или выполнить другие необходимые действия
            // ...

            // После обработки сообщений об изменении цен, вы можете выполнить другие действия или обновить интерфейс
            // ...
        }

        public AppSettings LoadAppSettingsFromFile()
        {
            string filePath = "appSettings.json";

            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<AppSettings>(jsonContent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке настроек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return new AppSettings();
        }

    }
}

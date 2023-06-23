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


        }

        // Получаю клиента для работы
        private async void InitializeClientAsync()
        {
            _client = await Creaters.CreateClientAsync();
        }

        public async Task<List<Instrument>> LoadTickersFromFileAsync()
        {
            List<Instrument> instruments = new List<Instrument>();
            string filePath = "appSettings.json";
            try
            {
                string[] tickerLines = await File.ReadAllLinesAsync(filePath);

                foreach (string ticker in tickerLines)
                {
                    instruments.Add(new Instrument { Figi = ticker });
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки чтения файла настроек
                // Можно выбросить исключение или выполнить другую логику обработки ошибки
                MessageBox.Show($"Ошибка чтения файла настроек: {ex.Message}");
            }

            return instruments;
        }

    }
}

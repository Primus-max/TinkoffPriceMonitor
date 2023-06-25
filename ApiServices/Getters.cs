using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace TinkoffPriceMonitor.ApiServices
{
    public class Getters
    {
        public static async Task<decimal> GetUpdatedPrice(Share instrument, InvestApiClient client)
        {
            try
            {
                var request = new GetLastPricesRequest()
                {
                    Figi = { instrument.Figi },
                };

                var response = await client.MarketData.GetLastPricesAsync(request);

                var updatedInstrument = response.LastPrices.FirstOrDefault();
                if (updatedInstrument != null && updatedInstrument.Price != null)
                {
                    return updatedInstrument.Price;
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки при обращении к API
                MessageBox.Show($"Ошибка при получении обновленной цены. Причина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return 0; // Возвращаем значение по умолчанию, если не удалось получить цену или возникла ошибка
        }

    }
}

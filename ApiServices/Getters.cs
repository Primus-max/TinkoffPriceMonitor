using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;

namespace TinkoffPriceMonitor.ApiServices
{
    public class Getters
    {
        public static async Task<decimal> GetUpdatedPrice(Share instrument, InvestApiClient client)
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

            return 0; // Возвращаем значение по умолчанию, если не удалось получить цену
        }

    }
}

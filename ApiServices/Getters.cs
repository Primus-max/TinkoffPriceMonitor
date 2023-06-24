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
        public static async Task<IEnumerable<Instrument>> GetUpdatedPrices(IEnumerable<Instrument> instruments, InvestApiClient client)
        {
            var instrumentList = instruments.ToList();
            var figiList = instrumentList.Select(x => x.Figi);

            var request = new GetLastPricesRequest()
            {
                Figi = { figiList },
            };

            var response = client.MarketData.GetLastPrices(request);

            foreach (var instrument in instrumentList)
            {
                try
                {
                    var updatedInstrument = response.LastPrices.FirstOrDefault(x => x.Figi == instrument.Figi);
                    if (updatedInstrument != null && updatedInstrument.Price != null)
                    {
                        instrument.MinPriceIncrement = updatedInstrument.Price;
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogError(e, "Error occured while updating instruments prices");
                }
            }

            return instrumentList;
        }
    }
}

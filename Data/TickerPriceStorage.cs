using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class TickerPriceStorage
{
    private string filePath = "tickerPrices.dat";

    [Serializable]
    public class TickerGroup
    {
        public string GroupName { get; set; }
        public List<TickerPrice> Tickers { get; set; }
    }

    [Serializable]
    public class TickerPrice
    {
        public string Ticker { get; set; }
        public decimal Price { get; set; }
    }

    public void SaveTickerPrice(List<TickerGroup> tickerGroups)
    {
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter?.Serialize(stream, tickerGroups);
        }
    }

    public List<(string GroupName, TickerPrice Ticker)> LoadTickerPrice()
    {
        if (File.Exists(filePath))
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                var tickerGroups = (List<TickerGroup>)formatter.Deserialize(stream);

                return tickerGroups
                    .SelectMany(g => g.Tickers.Select(t => (g.GroupName, t)))
                    .ToList();
            }
        }
        else
        {
            return new List<(string GroupName, TickerPrice Ticker)>();
        }
    }

}

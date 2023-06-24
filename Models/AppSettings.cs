using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffPriceMonitor.Models
{
    public class TickerGroup
    {
        public List<string>? Tickers { get; set; }
        public double PercentageThreshold { get; set; }
        public int Interval { get; set; }
    }

    public class AppSettings
    {
        public List<TickerGroup>? TickerGroups { get; set; }

    }

}

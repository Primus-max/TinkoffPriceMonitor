using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffPriceMonitor.Models
{
    public class TickerGroup
    {
        public string? GroupName { get; set; }
        public List<string>? Tickers { get; set; }
        public double PercentageThreshold { get; set; }
        public int Interval { get; set; }
    }
}

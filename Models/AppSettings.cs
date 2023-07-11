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
        public string? Tickers { get; set; }
        public decimal PercentageThreshold { get; set; }
        public int Interval { get; set; }
        public decimal OrderAmountRubles { get; set; }
    }
}

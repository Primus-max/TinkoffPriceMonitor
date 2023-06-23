using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffPriceMonitor.Models
{
    public class AppSettings
    {
        public string? TickerGroup1 { get; set; }
        public double PercentageThresholdGroup1 { get; set; }
        public int IntervalGroup1 { get; set; }

        public string? TickerGroup2 { get; set; }
        public double PercentageThresholdGroup2 { get; set; }
        public int IntervalGroup2 { get; set; }

        public string? TickerGroup3 { get; set; }
        public double PercentageThresholdGroup3 { get; set; }
        public int IntervalGroup3 { get; set; }
    }

}

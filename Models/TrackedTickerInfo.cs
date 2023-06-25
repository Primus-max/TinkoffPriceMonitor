using System;

namespace TinkoffPriceMonitor.Models
{
    public class TrackedTickerInfo
    {
        public string? GroupName { get; set; }
        public string? TickerName { get; set; }
        public decimal Price { get; set; }
        public decimal PriceChangePercentage { get; set; }
        public DateTime EventTime { get; set; }
    }

}

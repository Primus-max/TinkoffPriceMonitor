using System;

namespace TinkoffPriceMonitor.Models
{
    public class TrackedTickerInfo
    {
        public string? TickerName { get; set; }
        public decimal PriceChangePercentage { get; set; }
        public DateTime EventTime { get; set; }
    }
}

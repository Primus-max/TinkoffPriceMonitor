using System;
using TinkoffPriceMonitor.ViewModels.BaseView;

namespace TinkoffPriceMonitor.Models
{
    public class TrackedTickerInfo : BaseViewModel
    {
        public string? GroupName { get; set; }
        public string? TickerName { get; set; }
        public decimal Price { get; set; }
        public decimal PriceChangePercentage { get; set; }
        public DateTime EventTime { get; set; }
        public bool IsPositivePriceChange { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinkoffPriceMonitor.Models
{
    public class PriceChangeMessage
    {
        public string? InstrumentName { get; set; }
        public string? MessageText { get; set; }
    }

}

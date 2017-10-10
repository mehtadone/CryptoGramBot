using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BittrexSharp.Domain
{
    public class OrderBook
    {
        public string MarketName { get; set; }
        public IEnumerable<OrderBookEntry> Buy { get; set; }
        public IEnumerable<OrderBookEntry> Sell { get; set; }
    }
}

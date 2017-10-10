using System;
using System.Collections.Generic;
using System.Text;

namespace BittrexSharp.Domain
{
    public class OrderBookEntry
    {
        public decimal Quantity { get; set; }
        public decimal Rate { get; set; }
    }
}

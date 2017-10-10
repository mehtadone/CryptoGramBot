using System;
using System.Collections.Generic;
using System.Text;

namespace BittrexSharp.Domain
{
    public class Ticker
    {
        public string MarketName { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
        public decimal? Last { get; set; }
    }
}

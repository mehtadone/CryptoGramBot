using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public class MarketTrade
    {
        public int Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public FillType FillType { get; set; }
        public OrderType OrderType { get; set; }
    }
}

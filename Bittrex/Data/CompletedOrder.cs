using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public class CompletedOrder
    {
        public string OrderUuid { get; set; }
        public string Exchange { get; set; }
        public DateTime TimeStamp { get; set; }
        public OpenOrderType OrderType { get; set; }
        public decimal Limit { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal Commission { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsConditional { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }
        public bool ImmediateOrCancel { get; set; }
    }
}

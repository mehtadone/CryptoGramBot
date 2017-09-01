using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bittrex;

namespace CryptoGramBot.Models
{
    public class Trade
    {
        public string Base { get; set; }
        public decimal Commission { get; set; }
        public decimal Cost { get; set; }
        public string Exchange { get; set; }
        public string Id { get; set; }
        public decimal Limit { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public TradeSide Side { get; set; }
        public string Terms { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
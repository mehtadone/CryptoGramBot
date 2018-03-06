using System;

namespace CryptoGramBot.Models
{
    public class Trade
    {
        public string Base { get; set; }
        public decimal Commission { get; set; }
        public decimal Cost { get; set; }
        public string Exchange { get; set; }
        public string ExchangeId { get; set; }
        public int Id { get; set; }
        public decimal Limit { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityOfTrade => Quantity - QuantityRemaining;
        public decimal QuantityRemaining { get; set; }
        public TradeSide Side { get; set; }
        public string Terms { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
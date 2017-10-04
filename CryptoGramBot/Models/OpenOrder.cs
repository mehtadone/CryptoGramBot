using System;

namespace CryptoGramBot.Models
{
    public class OpenOrder
    {
        public string Base { get; set; }
        public bool CancelInitiated { get; set; }
        public decimal CommissionPaid { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }
        public string Exchange { get; set; }
        public int Id { get; set; }
        public bool ImmediateOrCancel { get; set; }
        public bool IsConditional { get; set; }
        public decimal Limit { get; set; }
        public DateTime Opened { get; set; }
        public string OrderUuid { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public TradeSide Side { get; set; }
        public string Terms { get; set; }
        public string Uuid { get; set; }
    }
}
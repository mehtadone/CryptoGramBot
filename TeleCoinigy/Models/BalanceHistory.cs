using System;

namespace TeleCoinigy.Models
{
    public class BalanceHistory
    {
        public decimal Balance { get; set; }
        public DateTime DateTime { get; set; }
        public decimal DollarAmount { get; set; }
        public string Name { get; set; }
    }
}
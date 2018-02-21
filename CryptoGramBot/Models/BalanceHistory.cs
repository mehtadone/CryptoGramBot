using System;
using System.ComponentModel.DataAnnotations;

namespace CryptoGramBot.Models
{
    public class BalanceHistory
    {
        public decimal Balance { get; set; }
        public DateTime DateTime { get; set; }
        public decimal DollarAmount { get; set; }

        [Key]
        public int Key { get; set; }

        public string Name { get; set; }
    }
}
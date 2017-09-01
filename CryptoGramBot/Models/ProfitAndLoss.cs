using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Models
{
    public class ProfitAndLoss
    {
        public decimal AverageBuyPrice { get; set; }
        public decimal AverageSellPrice { get; set; }
        public string Base { get; set; }
        public decimal CommissionPaid { get; set; }
        public decimal DollarProfit { get; set; }
        public string Pair => Base + "-" + Terms;
        public decimal Profit { get; set; }
        public decimal QuantityBought { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal Remaining => QuantityBought - QuantitySold;
        public string Terms { get; set; }
        public decimal UnrealisedProfit { get; set; }
    }
}
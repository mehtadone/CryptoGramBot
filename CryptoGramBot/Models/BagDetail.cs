using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Models
{
    public class BagDetail
    {
        public decimal BoughtPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public WalletBalance WalletBalance { get; set; }
    }
}
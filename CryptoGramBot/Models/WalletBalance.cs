using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Models
{
    public class WalletBalance
    {
        public decimal Address { get; set; }
        public decimal Available { get; set; }
        public decimal Balance { get; set; }
        public decimal BtcAmount { get; set; }
        public string Currency { get; set; }
        public string Exchange { get; set; }
        public decimal Pending { get; set; }
        public decimal Price { get; set; }
        public decimal Requested { get; set; }
        public DateTime Timestamp { get; set; }
        public string Uuid { get; set; }
    }
}
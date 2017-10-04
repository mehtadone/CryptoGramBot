using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Poloniex;

namespace CryptoGramBot.Models
{
    public class Deposit
    {
        public string Address { get; set; }
        public double Amount { get; set; }
        public uint Confirmations { get; set; }
        public string Currency { get; set; }
        public string Exchange { get; set; }
        public int Id { get; set; }
        public string Status { get; set; }

        public DateTime Time { get; set; }

        public string TransactionId { get; set; }
    }
}
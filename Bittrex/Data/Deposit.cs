using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex.Data
{
    public class Deposit
    {
        public decimal Amount { get; set; }
        public int Confirmations { get; set; }
        public string CryptoAddress { get; set; }
        public string Currency { get; set; }
        public ulong Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public string TxId { get; set; }
    }
}
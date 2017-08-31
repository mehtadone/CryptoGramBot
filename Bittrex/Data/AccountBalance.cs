using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public class AccountBalance
    {
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal Available { get; set; }
        public decimal Pending { get; set; }
        public string CryptoAddress { get; set; }
        public bool Requested { get; set; }
        public string Uuid { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex.Data
{
    public class Withdrawal
    {
        public string Address { get; set; }
        public double Amount { get; set; }
        public bool Authorized { get; set; }
        public bool Canceled { get; set; }
        public string Currency { get; set; }
        public bool InvalidAddress { get; set; }
        public DateTime Opened { get; set; }
        public string PaymentUuid { get; set; }
        public bool PendingPayment { get; set; }
        public double TxCost { get; set; }
        public object TxId { get; set; }
    }
}
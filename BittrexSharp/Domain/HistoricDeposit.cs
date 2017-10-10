using System;
using System.Collections.Generic;
using System.Text;

namespace BittrexSharp.Domain
{
    public class HistoricDeposit
    {
        public string PaymentUuid { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public DateTime Opened { get; set; }
        public bool Authorized { get; set; }
        public bool PendingPayment { get; set; }
        public decimal TxCost { get; set; }
        public string TxId { get; set; }
        public bool Canceled { get; set; }
        public bool InvalidAddress { get; set; }
    }
}

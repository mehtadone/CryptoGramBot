using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public class ExchangeContext
    {
        public string ApiKey { get; set; }
        public string Secret { get; set; }
        public string QuoteCurrency  { get; set; }
        public bool Simulate { get; set; }
    }
}

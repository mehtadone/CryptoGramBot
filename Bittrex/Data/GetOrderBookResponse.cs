using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public class GetOrderBookResponse
    {
        public List<OrderEntry> buy { get; set; }
        public List<OrderEntry> sell { get; set; }
    }
}

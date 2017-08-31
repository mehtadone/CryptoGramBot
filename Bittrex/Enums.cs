using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    public enum FillType
    {
        Fill,
        Partial_Fill
    }

    public enum OpenOrderType
    {
        LIMIT_BUY,
        LIMIT_SELL
    }

    public enum OrderBookType
    {
        Buy,
        Sell,
        Both
    }

    public enum OrderType
    {
        Buy,
        Sell
    }
}
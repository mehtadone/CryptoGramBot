using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    [Flags]
    [Serializable]
    public enum TradeFlags
    {
        None = 0,
        PostOnly = 1,
        ImmediateOrCancel = 2
    }
}

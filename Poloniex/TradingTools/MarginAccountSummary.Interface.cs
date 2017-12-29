using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public interface IMarginAccountSummary
    {
        double TotalValue { get; }
        double ProfitLoss { get; }
        double LendingFees { get; }
        double NetValue { get; }
        double TotalBorrowedValue { get; }
        double CurrentMargin { get; }
    }
}

using System.Collections.Generic;
using TeleCoinigy.Models;

namespace TeleCoinigy.Helpers
{
    public static class ProfitCalculator
    {
        public static ProfitAndLoss GetProfitAndLoss(IEnumerable<Trade> trades, string ccy1, string ccy2)
        {
            decimal totalBought = 0;
            decimal totalSold = 0;

            decimal totalBuyCost = 0;
            decimal totalSellCost = 0;
            decimal commisssion = 0;
            foreach (var trade in trades)
            {
                if (trade.Side == TradeSide.Buy)
                {
                    totalBought = totalBought + trade.Quantity;
                    totalBuyCost = totalBuyCost + trade.Cost;
                }

                if (trade.Side == TradeSide.Sell)
                {
                    totalSold = totalSold + trade.Quantity;
                    totalSellCost = totalSellCost + trade.Cost;
                }

                commisssion = commisssion + trade.Commission;
            }

            var profitAndLoss = new ProfitAndLoss
            {
                AverageBuyPrice = totalBuyCost / totalBought,
                AverageSellPrice = totalBuyCost / totalSold,
                Profit = totalBuyCost - totalSellCost,
                Base = ccy1,
                Terms = ccy2,
                QuantitySold = totalSold,
                QuantityBought = totalBought,
                CommissionPaid = commisssion
            };

            return profitAndLoss;
        }
    }
}
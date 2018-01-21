using System;
using System.Collections.Generic;
using System.Linq;
using CryptoGramBot.Models;

namespace CryptoGramBot.Helpers
{
    public static class ProfitCalculator
    {
        public static decimal GetAveragePrice(List<Trade> onlyBuys, decimal quantity)
        {
            if (!onlyBuys.Any())
            {
                return 0m;
            }

            decimal cost = 0m;
            decimal quanitityRemaining = quantity;

            foreach (var trade in onlyBuys)
            {
                if (quanitityRemaining == 0) break;

                if (quanitityRemaining - trade.QuantityOfTrade <= 0)
                {
                    //test for partials
                    var price = trade.Cost / trade.QuantityOfTrade;
                    cost = cost + (quanitityRemaining * price);
                    quanitityRemaining = 0;
                }
                else
                {
                    quanitityRemaining = quanitityRemaining - trade.QuantityOfTrade;
                    cost = cost + trade.Cost;
                }
            }

            if (quanitityRemaining != 0)
            {
                cost = cost + (quanitityRemaining * (onlyBuys.First().Cost / onlyBuys.First().QuantityOfTrade));
            }

            return cost / quantity;
        }

        public static ProfitAndLoss GetProfitAndLossForPair(IEnumerable<Trade> trades, Currency currency)
        {
            var tradeList = trades.ToList();

            decimal totalBought = 0;
            decimal totalSold = 0;

            decimal totalBuyCost = 0;
            decimal totalSellCost = 0;
            decimal commisssion = 0;
            foreach (var trade in tradeList)
            {
                if (trade.Side == TradeSide.Buy)
                {
                    totalBought = totalBought + trade.QuantityOfTrade;
                    totalBuyCost = totalBuyCost + trade.Cost;
                }

                if (trade.Side == TradeSide.Sell)
                {
                    totalSold = totalSold + trade.QuantityOfTrade;
                    totalSellCost = totalSellCost + trade.Cost;
                }
                commisssion = commisssion + trade.Commission;
            }

            decimal averageBuy = 0;
            decimal averageSell = 0;
            try
            {
                averageBuy = totalBuyCost / totalBought;
                averageSell = totalSellCost / totalSold;
            }
            catch (Exception)
            {
                // TODO: Should log a could not divide by 0;
            }

            var remaining = totalBought - totalSold;

            var profitAndLoss = new ProfitAndLoss
            {
                AverageBuyPrice = averageBuy,
                AverageSellPrice = averageSell,
                UnrealisedProfit = totalSellCost - totalBuyCost,
                Profit = (totalSellCost - totalBuyCost) - (averageBuy * remaining),
                Base = currency.Base,
                Terms = currency.Terms,
                QuantitySold = totalSold,
                QuantityBought = totalBought,
                CommissionPaid = commisssion
            };

            return profitAndLoss;
        }

        public static decimal GetProfitForSell(decimal sellReturns, decimal quantitiy, decimal averageBuyPrice, decimal totalCost)
        {
            var profit = Math.Round((sellReturns - (averageBuyPrice * quantitiy)) / totalCost * 100, 3,
                MidpointRounding.ToEven);
            return profit;
        }

        public static decimal PriceDifference(decimal currentPrice, decimal limit)
        {
            if (limit == 0)
            {
                return 0;
            }

            var percentage = (currentPrice - limit) / limit * 100;

            return Math.Round(percentage, 2);
        }
    }
}
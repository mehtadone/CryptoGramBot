using System;
using System.Collections.Generic;
using CryptoGramBot.Models;

namespace CryptoGramBot.Helpers
{
    public static class ProfitCalculator
    {
        public static ProfitAndLoss GetProfitAndLossForPair(IEnumerable<Trade> trades, string ccy1, string ccy2)
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

            decimal averageBuy = 0;
            decimal averageSell = 0;
            try
            {
                averageBuy = totalBuyCost / totalBought;
                averageSell = totalSellCost / totalSold;
            }
            catch (Exception ex)
            {
                // TODO: Should log a could not divide by 0;
            }

            var profitAndLoss = new ProfitAndLoss
            {
                AverageBuyPrice = averageBuy,
                AverageSellPrice = averageSell,
                Profit = totalBuyCost - totalSellCost,
                Base = ccy1,
                Terms = ccy2,
                QuantitySold = totalSold,
                QuantityBought = totalBought,
                CommissionPaid = commisssion
            };

            return profitAndLoss;
        }

        public static decimal? GetProfitForTrade(List<Trade> trades, decimal sellReturns, decimal quantity)
        {
            var quantityChecked = 0m;
            var totalcost = 0m;
            foreach (var trade in trades)
            {
                if (quantityChecked >= quantity) break;
                if (quantityChecked + trade.Quantity > quantity)
                {
                    var quantityLeft = quantity - quantityChecked;
                    var cost = trade.Limit * quantityLeft;
                    totalcost = totalcost + cost;
                    quantityChecked = quantityChecked + quantityLeft;
                }
                else if (trade.Quantity < quantity)
                {
                    totalcost = totalcost + trade.Cost;
                    quantityChecked = quantityChecked + trade.Quantity;
                }
            }

            if (quantityChecked == 0m) return null;

            return Math.Round((sellReturns - totalcost) / totalcost * 100, 3, MidpointRounding.ToEven);
        }

        public static decimal PriceDifference(decimal currentPrice, decimal limit)
        {
            var percentage = (currentPrice - limit) / limit * 100;
            return Math.Round(percentage, 2);
        }
    }
}
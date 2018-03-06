using System;
using System.Collections.Generic;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using NUnit.Framework;

namespace CryptoGramBot.Tests
{
    [TestFixture]
    public class ProfitAndLossTests
    {
        private List<Trade> _trades;

        [Test]
        public void TestPerTradeSellPnL()
        {
            _trades = new List<Trade>
            {
                CreateTrade("BTC", "LTC", 0.000M, 500m, 7500m, 15, TradeSide.Buy, DateTime.Now),
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 10000m, 10, TradeSide.Buy, DateTime.Now - TimeSpan.FromDays(-1)),
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 15000m, 19, TradeSide.Buy, DateTime.Now - TimeSpan.FromDays(-2)),
            };

            var averagePrice = ProfitCalculator.GetAveragePrice(_trades, 19);

            Assert.AreEqual(605.26, Math.Round(averagePrice, 2));
        }

        [Test]
        public void TestPnL()
        {
            var profitAndLoss = ProfitCalculator.GetProfitAndLossForPair(_trades, new Currency { Base = "BTC", Terms = "LTC" });

            Assert.AreEqual(150, profitAndLoss.QuantityBought);
            Assert.AreEqual(100, profitAndLoss.QuantitySold);
            Assert.AreEqual(-0.016666666666666666666666665m, profitAndLoss.Profit);
            Assert.AreEqual(0.0001333333333333333333333333M, profitAndLoss.AverageBuyPrice);
            Assert.AreEqual(0.0001M, profitAndLoss.AverageSellPrice);
            Assert.AreEqual(0.002M, profitAndLoss.CommissionPaid);
        }

        [Test]
        public void TestSellProfit()
        {
            _trades = new List<Trade>
                        {
                            CreateTrade("BTC", "XPM", 0.0025M, 0.00003911M, 0.01083815M, 277.11967321M, TradeSide.Buy,
                                new DateTime(2017, 09, 29, 20, 13, 5)),
                            CreateTrade("BTC", "XPM", 0.0025M, 0.00003980M, 0.00552158M, 138.73325317M, TradeSide.Buy,
                                new DateTime(2017, 09, 29, 19, 22, 56)),
                            CreateTrade("BTC", "XPM", 0.0025M, 0.00004084M, 0.00568006M, 139.08095555M, TradeSide.Buy,
                                new DateTime(2017, 09, 29, 19, 22, 56)),
                        };

            var quanititySold = 300m;
            var averageBuyPrice = ProfitCalculator.GetAveragePrice(_trades, quanititySold);
            var profitPercent = ProfitCalculator.GetProfitForSell(0.02M, quanititySold, averageBuyPrice, quanititySold * averageBuyPrice);

            Assert.AreEqual(70.23, profitPercent);
        }

        [SetUp]
        protected void SetUp()
        {
            _trades = new List<Trade>
            {
                CreateTrade("BTC", "LTC", 0.001M, 1.0M, 0.01M, 100, TradeSide.Sell, DateTime.Now),
                CreateTrade("BTC", "LTC", 0.001M, 1.0M, 0.02M, 150, TradeSide.Buy, DateTime.Now)
            };
        }

        private Trade CreateTrade(string ccy1, string ccy2, decimal commision, decimal limit, decimal cost, decimal quanity, TradeSide side, DateTime timeStamp)
        {
            var trade = new Trade
            {
                Base = ccy1,
                Terms = ccy2,
                Commission = commision,
                Cost = cost,
                Quantity = quanity,
                Side = side,
                Limit = limit,
                Timestamp = timeStamp
            };
            return trade;
        }
    }
}
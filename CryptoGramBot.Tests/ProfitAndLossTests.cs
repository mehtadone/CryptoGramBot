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
                CreateTrade("BTC", "LTC", 0.000M, 500m, 7500m, 15, TradeSide.Buy),
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 10000m, 10, TradeSide.Buy),
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 15000m, 19, TradeSide.Buy),
            };

            ProfitCalculator.GetProfitForTrade(_trades, 19800m, 18m, out decimal? totalCost, out decimal? profitAndLoss);

            Assert.AreEqual(89, profitAndLoss);
        }

        [Test]
        public void TestPnL()
        {
            var profitAndLoss = ProfitCalculator.GetProfitAndLossForPair(_trades, "BTC", "LTC");

            Assert.AreEqual(150, profitAndLoss.QuantityBought);
            Assert.AreEqual(100, profitAndLoss.QuantitySold);
            Assert.AreEqual(0.01M, profitAndLoss.Profit);
            Assert.AreEqual(0.0001333333333333333333333333M, profitAndLoss.AverageBuyPrice);
            Assert.AreEqual(0.0001M, profitAndLoss.AverageSellPrice);
            Assert.AreEqual(0.002M, profitAndLoss.CommissionPaid);
        }

        [SetUp]
        protected void SetUp()
        {
            _trades = new List<Trade>
            {
                CreateTrade("BTC", "LTC", 0.001M, 1.0M, 0.01M, 100, TradeSide.Sell),
                CreateTrade("BTC", "LTC", 0.001M, 1.0M, 0.02M, 150, TradeSide.Buy)
            };
        }

        private Trade CreateTrade(string ccy1, string ccy2, decimal commision, decimal limit, decimal cost, decimal quanity, TradeSide side)
        {
            var trade = new Trade
            {
                Base = ccy1,
                Terms = ccy2,
                Commission = commision,
                Cost = cost,
                Quantity = quanity,
                Side = side,
                Limit = limit
            };
            return trade;
        }
    }
}
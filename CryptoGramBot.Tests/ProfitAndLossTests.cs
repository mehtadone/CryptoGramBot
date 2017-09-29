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
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 10000m, 10, TradeSide.Buy, DateTime.Now),
                CreateTrade("BTC", "LTC", 0.000M, 1000m, 15000m, 19, TradeSide.Buy, DateTime.Now),
            };

            ProfitCalculator.GetProfitForTrade(_trades, 19800m, 18m, out decimal? totalCost, out decimal? profitAndLoss, out DateTime boughtdate);

            Assert.AreEqual(88.571, profitAndLoss);
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

        [Test]
        public void TestSellProfit()
        {
            _trades = new List<Trade>
            {
                CreateTrade("BTC", "XPM", 0.0025M, 0.00003980M, 0.00552158M, 138.73325317M, TradeSide.Buy,
                    new DateTime(2017, 09, 29, 19, 22, 56)),
                CreateTrade("BTC", "XPM", 0.0025M, 0.00003911M, 0.01083815M, 277.11967321M, TradeSide.Buy,
                    new DateTime(2017, 09, 29, 20, 13, 5)),
                CreateTrade("BTC", "XPM", 0.0025M, 0.00004084M, 0.00568006M, 139.08095555M, TradeSide.Buy,
                    new DateTime(2017, 09, 29, 19, 22, 56)),
            };

            ProfitCalculator.GetProfitForTrade(_trades, 0.02249510M, 553.54654724M, out decimal? totalCost, out decimal? profitPercent, out DateTime boughtDate);
            decimal? btcProfit = 0.02249510M - totalCost.Value;

            var dateTime = new DateTime(2017, 09, 29, 20, 13, 56);
            Assert.AreEqual(dateTime.Year, boughtDate.Year);
            Assert.AreEqual(dateTime.Month, boughtDate.Month);
            Assert.AreEqual(dateTime.Day, boughtDate.Day);
            Assert.AreEqual(dateTime.Second, boughtDate.Second);

            Assert.AreEqual(0.0219831374759224m, totalCost);
            Assert.AreEqual(0.0005119625240776m, btcProfit);
            Assert.AreEqual(2.329, profitPercent);
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
                TimeStamp = timeStamp
            };
            return trade;
        }
    }
}
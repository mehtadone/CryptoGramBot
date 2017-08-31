using System.Collections.Generic;
using NUnit.Framework;
using TeleCoinigy.Helpers;
using TeleCoinigy.Models;
using TeleCoinigy.Services;

namespace TeleCoinigy.Tests
{
    [TestFixture]
    public class ProfitAndLossTests
    {
        private List<Trade> _trades;

        [Test]
        public void TestPnL()
        {
            var profitAndLoss = ProfitCalculator.GetProfitAndLoss(_trades, "BTC", "LTC");

            Assert.AreEqual(150, profitAndLoss.QuantityBought);
            Assert.AreEqual(100, profitAndLoss.QuantitySold);
            Assert.AreEqual(0.01M, profitAndLoss.Profit);
            Assert.AreEqual(0.0001333333333333333333333333M, profitAndLoss.AverageBuyPrice);
            Assert.AreEqual(0.0002M, profitAndLoss.AverageSellPrice);
            Assert.AreEqual(0.002M, profitAndLoss.CommissionPaid);
        }

        [SetUp]
        protected void SetUp()
        {
            _trades = new List<Trade>
            {
                CreateTrade("BTC", "LTC", 0.001M, 0.01M, 100, TradeSide.Sell),
                CreateTrade("BTC", "LTC", 0.001M, 0.02M, 150, TradeSide.Buy)
            };
        }

        private Trade CreateTrade(string ccy1, string ccy2, decimal commision, decimal cost, decimal quanity, TradeSide side)
        {
            var trade = new Trade
            {
                Base = ccy1,
                Terms = ccy2,
                Commission = commision,
                Cost = cost,
                Quantity = quanity,
                Side = side
            };
            return trade;
        }
    }
}
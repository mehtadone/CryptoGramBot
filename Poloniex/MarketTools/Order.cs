using Jojatekok.PoloniexAPI;

namespace Poloniex.MarketTools
{
    public class Order : IOrder
    {
        internal Order(double pricePerCoin, double amountQuote)
        {
            PricePerCoin = pricePerCoin;
            AmountQuote = amountQuote;
        }

        internal Order()
        {
        }

        public double AmountBase => (AmountQuote * PricePerCoin).Normalize();

        public double AmountQuote { get; private set; }
        public double PricePerCoin { get; private set; }
    }
}
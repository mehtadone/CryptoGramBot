namespace Jojatekok.PoloniexAPI.MarketTools
{
    public class Order : IOrder
    {
        public double PricePerCoin { get; private set; }

        public double AmountQuote { get; private set; }
        public double AmountBase {
            get { return (AmountQuote * PricePerCoin).Normalize(); }
        }

        internal Order(double pricePerCoin, double amountQuote)
        {
            PricePerCoin = pricePerCoin;
            AmountQuote = amountQuote;
        }

        internal Order()
        {

        }
    }
}

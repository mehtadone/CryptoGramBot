// Special class to represent all currency pairs. To be used in Trade / Order history queries
namespace Poloniex.General
{
    internal sealed class AllCurrencyPairs : CurrencyPair
    {
        public AllCurrencyPairs(string baseCurrency, string quoteCurrency) : base(baseCurrency, quoteCurrency)
        {
        }

        public AllCurrencyPairs() : base("", "")
        {
        }

        public override string ToString()
        {
            return "all";
        }
    }
}
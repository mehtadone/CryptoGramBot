namespace CryptoGramBot.Configuration
{
    public class GeneralConfig
    {
        public string DatabaseLocation { get; set; }
        public decimal IgnoreDustInTradingCurrency { get; set; }
        public double TimeOffset { get; set; }

        public string TradingCurrency { get; set; }
    }
}
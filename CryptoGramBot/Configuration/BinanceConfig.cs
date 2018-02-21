namespace CryptoGramBot.Configuration
{
    public class BinanceConfig : IConfig
    {
        public decimal? BagNotification { get; set; }
        public bool? BuyNotifications { get; set; }
        public string DailyNotifications { get; set; }
        public bool? DepositNotification { get; set; }
        public decimal? DustNotification { get; set; }
        public bool Enabled { get; set; }
        public string Key { get; set; }
        public decimal? LowBtcNotification { get; set; }
        public bool? OpenOrderNotification { get; set; }
        public string Secret { get; set; }
        public bool? SellNotifications { get; set; }
        public bool? SendHourlyUpdates { get; set; }
        public bool? WithdrawalNotification { get; set; }
    }
}
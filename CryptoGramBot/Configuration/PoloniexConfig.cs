namespace CryptoGramBot.Configuration
{
    public class PoloniexConfig : IConfig
    {
        public bool BuyNotifications { get; set; }
        public bool DepositNotification { get; set; }
        public bool Enabled { get; set; }
        public string Key { get; set; }
        public bool OpenOrderNotification { get; set; }
        public string Secret { get; set; }
        public bool SellNotifications { get; set; }
        public bool SendHourlyUpdates { get; set; }
        public bool WithdrawalNotification { get; set; }
    }
}
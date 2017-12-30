namespace CryptoGramBot.Configuration
{
    public interface IConfig
    {
        decimal? BagNotification { get; set; }
        bool? BuyNotifications { get; set; }
        string DailyNotifications { get; set; }
        bool? DepositNotification { get; set; }
        decimal? DustNotification { get; set; }
        bool Enabled { get; set; }
        string Key { get; set; }
        decimal? LowBtcNotification { get; set; }
        bool? OpenOrderNotification { get; set; }
        string Secret { get; set; }
        bool? SellNotifications { get; set; }
        bool? SendHourlyUpdates { get; set; }
        bool? WithdrawalNotification { get; set; }
    }
}
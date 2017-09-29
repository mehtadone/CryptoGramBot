namespace CryptoGramBot.Configuration
{
    public interface IConfig
    {
        bool BuyNotifications { get; set; }
        bool Enabled { get; set; }
        string Key { get; set; }
        string Secret { get; set; }
        bool SellNotifications { get; set; }
        bool SendHourlyUpdates { get; set; }
    }
}
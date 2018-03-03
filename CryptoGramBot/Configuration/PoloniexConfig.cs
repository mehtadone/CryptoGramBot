using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using CryptoGramBot.Helpers;

namespace CryptoGramBot.Configuration
{
    public class PoloniexConfig : IConfig
    {
        private readonly ILogger<PoloniexConfig> _log;

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

        public PoloniexConfig(ILogger<PoloniexConfig> log)
        {
            _log = log;
        }

        public bool IsValid()
        {
            bool result = true;

            if (Enabled)
            {
                if (string.IsNullOrEmpty(Key) || (Key == Constants.ConfigDummyValue))
                {
                    result = false;
                    _log.LogError($"Key is invalid or missing in Poloniex config");
                }

                if (string.IsNullOrEmpty(Secret) || (Secret == Constants.ConfigDummyValue))
                {
                    result = false;
                    _log.LogError($"Secret is invalid or missing in Poloniex config");
                }

                if (!string.IsNullOrEmpty(DailyNotifications) && Regex.Matches(DailyNotifications, @"[0-9]:[0-9]").Count == 0)
                {
                    result = false;
                    _log.LogError($"Invalid DailyNotifications [{DailyNotifications}] in Poloniex config - should be empty or specify a time, example 08:00");
                }
            }

            return result;
        }
    }
}
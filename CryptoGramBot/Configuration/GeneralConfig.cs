using System;
using Microsoft.Extensions.Logging;
using CryptoGramBot.Helpers;

namespace CryptoGramBot.Configuration
{
    public class GeneralConfig
    {
        private readonly ILogger<GeneralConfig> _log;

        public string DatabaseLocation { get; set; }
        public decimal IgnoreDustInTradingCurrency { get; set; }
        public double TimeOffset { get; set; }

        public string TradingCurrency { get; set; }
        public string ReportingCurrency { get; set; }

        public GeneralConfig(ILogger<GeneralConfig> log)
        {
            _log = log;
        }

        public bool IsValid()
        {
            bool result = true;

            if (string.IsNullOrEmpty(DatabaseLocation) || (DatabaseLocation == Constants.ConfigDummyValue))
            {
                result = false;
                _log.LogError($"DatabaseLocation is invalid or missing in General config");
            }

            if (string.IsNullOrEmpty(TradingCurrency) || (TradingCurrency == Constants.ConfigDummyValue))
            {
                result = false;
                _log.LogError($"TradingCurrency is invalid or missing in General config");
            }

            if (string.IsNullOrEmpty(ReportingCurrency) || (ReportingCurrency == Constants.ConfigDummyValue))
            {
                result = false;
                _log.LogError($"ReportingCurrency is invalid or missing in General config");
            }
            else
            {
                string[] supported = new string[] { "USD", "EUR", "GBP", "JPY", "KRW" };
                if (Array.IndexOf(supported, ReportingCurrency) < 0)
                {
                    result = false;
                    _log.LogError($"Unsupported ReportingCurrency [{ReportingCurrency}] in General config - supported values are: {string.Join(", ", supported)}");
                }
            }

            return result;
        }
    }
}
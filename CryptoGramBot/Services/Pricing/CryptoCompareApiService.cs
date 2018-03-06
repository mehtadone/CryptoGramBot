using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoCompare;

namespace CryptoGramBot.Services.Pricing
{
    public class CryptoCompareApiService : IPriceService
    {
        private readonly ILogger<CryptoCompareApiService> _log;

        public CryptoCompareApiService(ILogger<CryptoCompareApiService> log)
        {
            _log = log;
        }

        public async Task<decimal> GetReportingAmount(string baseCcy, decimal baseAmount, string reportingCurrency)
        {
            var price = await GetPrice(reportingCurrency, baseCcy);
            return Math.Round(price * baseAmount, 3);
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            if (Helpers.Helpers.CurrenciesAreEquivalent(baseCcy, termsCurrency))
            {
                return 1;
            }

            try
            {
                decimal price = 0;
                var priceResponse = await CryptoCompareClient.Instance.Prices.SingleAsync(termsCurrency, new string[] { baseCcy });
                priceResponse.TryGetValue(baseCcy, out price);
                return price;
            }
            catch (Exception e)
            {
                _log.LogError($"Error in getting {baseCcy}-{termsCurrency} price from CryptoCompare API: {e.Message}");
                return 0;
            }
        }
    }
}

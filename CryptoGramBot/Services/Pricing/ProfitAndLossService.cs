using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Pricing;

namespace CryptoGramBot.Services
{
    public class ProfitAndLossService
    {
        private readonly DatabaseService _databaseService;
        private readonly PriceService _priceService;
        private readonly GeneralConfig _config;

        public ProfitAndLossService(
            PriceService priceService,
            DatabaseService databaseService,
            GeneralConfig config)
        {
            _priceService = priceService;
            _databaseService = databaseService;
            _config = config;
        }

        public async Task<ProfitAndLoss> GetPnLInfo(string ccy1, string ccy2, string exchange)
        {
            var tradesForPair = await _databaseService.GetTradesForPair(ccy1, ccy2);
            var profitAndLoss = ProfitCalculator.GetProfitAndLossForPair(tradesForPair, new Currency { Base = ccy1, Terms = ccy2 });

            var reportingCurrency = _config.ReportingCurrency;
            var reportingAmount = await _priceService.GetReportingAmount(ccy1, profitAndLoss.Profit, reportingCurrency, exchange);

            profitAndLoss.ReportingProfit = reportingAmount;
            profitAndLoss.ReportingCurrency = reportingCurrency;

            await _databaseService.SaveProfitAndLoss(profitAndLoss);

            return profitAndLoss;
        }
    }
}
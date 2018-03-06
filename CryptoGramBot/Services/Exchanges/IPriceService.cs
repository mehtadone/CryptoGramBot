using System.Threading.Tasks;

namespace CryptoGramBot.Services
{
    public interface IPriceService
    {
        Task<decimal> GetReportingAmount(string baseCcy, decimal baseAmount, string reportingCurrency);
        Task<decimal> GetPrice(string baseCcy, string termsCurrency);
    }
}

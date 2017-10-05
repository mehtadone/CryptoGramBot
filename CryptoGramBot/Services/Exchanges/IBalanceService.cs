using System.Threading.Tasks;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public interface IBalanceService
    {
        Task<BalanceInformation> GetBalance();
    }
}
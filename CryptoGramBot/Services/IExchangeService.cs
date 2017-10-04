using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public interface IExchangeService : IBalanceService
    {
        Task<List<Deposit>> GetNewDeposits();

        Task<List<Withdrawal>> GetNewWithdrawals();

        Task<List<Trade>> GetOrderHistory(DateTime lastChecked);

        Task<decimal> GetPrice(string terms);
        Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked);
    }
}
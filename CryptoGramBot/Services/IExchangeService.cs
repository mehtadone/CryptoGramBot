using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public interface IExchangeService : IBalanceService
    {
        Task<List<Trade>> GetOrderHistory(DateTime lastChecked);
    }
}
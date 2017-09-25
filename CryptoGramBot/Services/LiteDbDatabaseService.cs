using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services
{
    public class LiteDbDatabaseService
    {
        private readonly LiteRepository _db;
        private readonly ILogger<LiteDbDatabaseService> _log;

        public LiteDbDatabaseService(ILogger<LiteDbDatabaseService> log, GeneralConfig config)
        {
            _log = log;
            _db = new LiteRepository(config.DatabaseLocation);
        }

        public void Close()
        {
            _db.Dispose();
        }

        public IEnumerable<BalanceHistory> GetAllBalances()
        {
            var liteCollection = _db.Database.GetCollection<BalanceHistory>();
            return liteCollection.FindAll();
        }

        public IEnumerable<LastChecked> GetAllLastChecked()
        {
            var liteCollection = _db.Database.GetCollection<LastChecked>();
            return liteCollection.FindAll();
        }

        public IEnumerable<ProfitAndLoss> GetAllProfitAndLoss()
        {
            var liteCollection = _db.Database.GetCollection<ProfitAndLoss>();
            return liteCollection.FindAll();
        }

        public IEnumerable<Trade> GetAllTrades()
        {
            var liteCollection = _db.Database.GetCollection<Trade>();
            return liteCollection.FindAll();
        }
    }
}
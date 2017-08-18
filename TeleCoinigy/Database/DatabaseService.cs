using System;
using System.Linq;
using LiteDB;
using TeleCoinigy.Models;

namespace TeleCoinigy.Database
{
    public class DatabaseService
    {
        public void AddBalance(double balance, string name)
        {
            var balanceHistory = new BalanceHistory
            {
                DateTime = DateTime.Now,
                Balance = balance
            };

            SaveBalance(balanceHistory, name);
        }

        public double GetLastBalance(string name)
        {
            using (var db = new LiteDatabase(@"coinigyBalances.db"))
            {
                var balances = db.GetCollection<BalanceHistory>("balances");
                var histories = balances.Find(Query.All(Query.Descending), limit: 1)
                    .Where(x => x.Name == name);

                var balanceHistories = histories as BalanceHistory[] ?? histories.ToArray();
                return !balanceHistories.Any() ? 0 : balanceHistories[0].Balance;
            }
        }

        private void SaveBalance(BalanceHistory balanceHistory, string name)
        {
            balanceHistory.Name = name;
            using (var db = new LiteDatabase(@"coinigyBalances.db"))
            {
                var balances = db.GetCollection<BalanceHistory>("balances");
                balances.Insert(balanceHistory);
            }
        }
    }
}
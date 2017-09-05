using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Database;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using OfficeOpenXml;

namespace CryptoGramBot.Services
{
    public class BalanceService
    {
        private readonly CoinigyApiService _coinigyApiService;
        private readonly DatabaseService _databaseService;

        public BalanceService(CoinigyApiService coinigyApiService, DatabaseService databaseService)
        {
            _coinigyApiService = coinigyApiService;
            _databaseService = databaseService;
        }

        public void AddTrades(IEnumerable<Trade> trades, out List<Trade> newTrades)
        {
            _databaseService.AddTrades(trades, out newTrades);
        }

        public async Task<BalanceInformation> Get24HourTotalBalance()
        {
            var hour24Balance = _databaseService.GetBalance24HoursAgo(Constants.CoinigyBalance);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance();

            var dollarAmount = await GetDollarAmount(balanceCurrent);
            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, Constants.CoinigyBalance);
            return new BalanceInformation(currentBalance, hour24Balance, Constants.CoinigyBalance);
        }

        public async Task<BalanceInformation> GetAccountBalance(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];
            var balance = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);

            var dollarAmount = await GetDollarAmount(balance);

            var lastBalance = _databaseService.GetLastBalance(selectedAccount.Name);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, selectedAccount.Name);

            return new BalanceInformation(currentBalance, lastBalance, selectedAccount.Name);
        }

        public async Task<BalanceInformation> GetAccountBalance24HoursAgo(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];
            var balance24HoursAgo = _databaseService.GetBalance24HoursAgo(selectedAccount.Name);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);

            var dollarAmount = await GetDollarAmount(balanceCurrent);
            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, selectedAccount.Name);
            return new BalanceInformation(currentBalance, balance24HoursAgo, selectedAccount.Name);
        }

        public async Task<Dictionary<int, Account>> GetAccounts()
        {
            var accounts = await _coinigyApiService.GetAccounts();
            return accounts;
        }

        public async Task<List<BalanceInformation>> GetAllBalances()
        {
            var balances = new List<BalanceInformation>();
            var accounts = await _coinigyApiService.GetAccounts();
            foreach (var account in accounts)
            {
                var accountBalance = await GetAccountBalance(account.Key);
                balances.Add(accountBalance);
            }

            return balances;
        }

        public async Task<ProfitAndLoss> GetPnLInfo(string ccy1, string ccy2)
        {
            var tradesForPair = _databaseService.GetTradesForPair(ccy1, ccy2);
            var profitAndLoss = ProfitCalculator.GetProfitAndLoss(tradesForPair, ccy1, ccy2);

            var dollarAmount = await GetDollarAmount(profitAndLoss.Profit);

            profitAndLoss.DollarProfit = dollarAmount;

            _databaseService.SaveProfitAndLoss(profitAndLoss);

            return profitAndLoss;
        }

        public async Task<BalanceInformation> GetTotalBalance()
        {
            var balance = await _coinigyApiService.GetBtcBalance();
            var lastBalance = _databaseService.GetLastBalance(Constants.CoinigyBalance);
            var dollarAmount = await GetDollarAmount(balance);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, Constants.CoinigyBalance);

            return new BalanceInformation(currentBalance, lastBalance, Constants.CoinigyBalance);
        }

        public FileInfo GetTradeExport()
        {
            using (var xlPackage = new ExcelPackage())
            {
                var pnlWorksheet = xlPackage.Workbook.Worksheets.Add("Profit and Loss");

                var allPairs = _databaseService.GetAllPairs();

                foreach (var terms in allPairs)
                {
                    var excelWorksheet = xlPackage.Workbook.Worksheets.Add(terms);

                    var allTradesForTerms = _databaseService.GetAllTradesFor(terms);
                    excelWorksheet.Cells["A1"].Value = "Id";
                    excelWorksheet.Cells["B1"].Value = "Timestamp";
                    excelWorksheet.Cells["C1"].Value = "Base";
                    excelWorksheet.Cells["D1"].Value = "Terms";
                    excelWorksheet.Cells["E1"].Value = "Side";
                    excelWorksheet.Cells["F1"].Value = "Limit";
                    excelWorksheet.Cells["G1"].Value = "Quantity";
                    excelWorksheet.Cells["H1"].Value = "Quantity Remaining";
                    excelWorksheet.Cells["I1"].Value = "Abs Quantity";
                    excelWorksheet.Cells["J1"].Value = "Cost";
                    excelWorksheet.Cells["K1"].Value = "Abs Cost";
                    excelWorksheet.Cells["L1"].Value = "Commission";
                    excelWorksheet.Cells["M1"].Value = "Exchange";

                    var i = 2;

                    foreach (var trade in allTradesForTerms)
                    {
                        var quanitiyAbs = trade.Quantity - trade.QuantityRemaining;
                        var costAbs = trade.Cost;
                        if (trade.Side == TradeSide.Buy)
                        {
                            quanitiyAbs = -quanitiyAbs;
                            costAbs = -costAbs;
                        }

                        excelWorksheet.Cells["A" + i].Value = trade.Id;
                        excelWorksheet.Cells["B" + i].Value = trade.TimeStamp;
                        excelWorksheet.Cells["C" + i].Value = trade.Base;
                        excelWorksheet.Cells["D" + i].Value = trade.Terms;
                        excelWorksheet.Cells["E" + i].Value = trade.Side;
                        excelWorksheet.Cells["F" + i].Value = trade.Limit;
                        excelWorksheet.Cells["G" + i].Value = trade.Quantity;
                        excelWorksheet.Cells["H" + i].Value = trade.QuantityRemaining;
                        excelWorksheet.Cells["I" + i].Value = quanitiyAbs;
                        excelWorksheet.Cells["J" + i].Value = trade.Cost;
                        excelWorksheet.Cells["K" + i].Value = costAbs;
                        excelWorksheet.Cells["L" + i].Value = trade.Commission;
                        excelWorksheet.Cells["M" + i].Value = trade.Exchange;

                        i++;
                    }
                }

                var path = Directory.GetCurrentDirectory() + @"\temp_trade_export.xlsx";
                var fileInfo = new FileInfo(path);
                xlPackage.SaveAs(fileInfo);

                return fileInfo;
            }
        }

        private async Task<decimal> GetDollarAmount(decimal balance)
        {
            var lastBid = await _coinigyApiService.GetTicker("BTC/USD");
            return Math.Round(lastBid * balance, 2);
        }
    }
}
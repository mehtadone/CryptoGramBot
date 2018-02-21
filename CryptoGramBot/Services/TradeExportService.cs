using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace CryptoGramBot.Services
{
    public class TradeExportService : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<TradeExportService> _log;

        public TradeExportService(ILogger<TradeExportService> log, DatabaseService databaseService)
        {
            _log = log;
            _databaseService = databaseService;
        }

        public void Dispose()
        {
        }

        public async Task<FileInfo> GetTradeExport()
        {
            using (var xlPackage = new ExcelPackage())
            {
                var pnlDict = new Dictionary<string, List<ProfitAndLoss>>();

                var pnlWorksheet = xlPackage.Workbook.Worksheets.Add("Pair PnL");
                var totalPnlWorksheet = xlPackage.Workbook.Worksheets.Add("Total PnL");

                var allPairs = await _databaseService.GetAllPairs();

                totalPnlWorksheet.Cells["A1"].Value = "Coin";
                totalPnlWorksheet.Cells["B1"].Value = "Unrealised Profit";
                totalPnlWorksheet.Cells["C1"].Value = "Realised Profit";

                pnlWorksheet.Cells["A1"].Value = "Base";
                pnlWorksheet.Cells["B1"].Value = "Terms";
                pnlWorksheet.Cells["C1"].Value = "Unrealised Profit in Base CCY";
                pnlWorksheet.Cells["D1"].Value = "Realised Profit in Base CCY";
                pnlWorksheet.Cells["E1"].Value = "Average Buy Price";
                pnlWorksheet.Cells["F1"].Value = "Average Sell Price";
                pnlWorksheet.Cells["G1"].Value = "Commission Paid";
                pnlWorksheet.Cells["H1"].Value = "Quantity Bought";
                pnlWorksheet.Cells["I1"].Value = "Quantity Sold";
                pnlWorksheet.Cells["J1"].Value = "Quantity Remaining";

                int lastPairProfitRow = 2;
                foreach (var currency in allPairs)
                {
                    var excelWorksheet = xlPackage.Workbook.Worksheets.Add(currency.ToString());

                    var allTradesForCurrency = _databaseService.GetAllTradesFor(currency);
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

                    foreach (var trade in allTradesForCurrency)
                    {
                        var costAbs = trade.Cost;
                        if (trade.Side == TradeSide.Buy)
                        {
                            costAbs = -costAbs;
                        }

                        excelWorksheet.Cells["A" + i].Value = trade.ExchangeId;
                        excelWorksheet.Cells["B" + i].Value = trade.Timestamp;
                        excelWorksheet.Cells["C" + i].Value = trade.Base;
                        excelWorksheet.Cells["D" + i].Value = trade.Terms;
                        excelWorksheet.Cells["E" + i].Value = trade.Side;
                        excelWorksheet.Cells["F" + i].Value = trade.Limit;
                        excelWorksheet.Cells["G" + i].Value = trade.Quantity;
                        excelWorksheet.Cells["H" + i].Value = trade.QuantityRemaining;
                        excelWorksheet.Cells["I" + i].Value = trade.QuantityOfTrade;
                        excelWorksheet.Cells["J" + i].Value = trade.Cost;
                        excelWorksheet.Cells["K" + i].Value = costAbs;
                        excelWorksheet.Cells["L" + i].Value = trade.Commission;
                        excelWorksheet.Cells["M" + i].Value = trade.Exchange;

                        i++;
                    }

                    ProfitAndLoss pnl = ProfitCalculator.GetProfitAndLossForPair(allTradesForCurrency, currency);
                    pnlWorksheet.Cells["A" + lastPairProfitRow].Value = pnl.Base;
                    pnlWorksheet.Cells["B" + lastPairProfitRow].Value = pnl.Terms;
                    pnlWorksheet.Cells["C" + lastPairProfitRow].Value = pnl.UnrealisedProfit;
                    pnlWorksheet.Cells["D" + lastPairProfitRow].Value = pnl.Profit;
                    pnlWorksheet.Cells["E" + lastPairProfitRow].Value = pnl.AverageBuyPrice;
                    pnlWorksheet.Cells["F" + lastPairProfitRow].Value = pnl.AverageSellPrice;
                    pnlWorksheet.Cells["G" + lastPairProfitRow].Value = pnl.CommissionPaid;
                    pnlWorksheet.Cells["H" + lastPairProfitRow].Value = pnl.QuantityBought;
                    pnlWorksheet.Cells["I" + lastPairProfitRow].Value = pnl.QuantitySold;
                    pnlWorksheet.Cells["J" + lastPairProfitRow].Value = pnl.Remaining;

                    if (pnlDict.Keys.Contains(pnl.Base))
                    {
                        pnlDict[pnl.Base].Add(pnl);
                    }
                    else
                    {
                        var list = new List<ProfitAndLoss> { pnl };
                        pnlDict[pnl.Base] = list;
                    }

                    lastPairProfitRow++;
                }

                int rowNumber = 2;
                foreach (var pnlPair in pnlDict)
                {
                    decimal realisedPnl = 0m;
                    decimal unrelaisedPnl = 0m;

                    foreach (var profitAndLoss in pnlPair.Value)
                    {
                        realisedPnl = realisedPnl + profitAndLoss.Profit;
                        unrelaisedPnl = realisedPnl + profitAndLoss.UnrealisedProfit;
                    }

                    totalPnlWorksheet.Cells["A" + rowNumber].Value = pnlPair.Key;
                    totalPnlWorksheet.Cells["B" + rowNumber].Value = unrelaisedPnl;
                    totalPnlWorksheet.Cells["C" + rowNumber].Value = realisedPnl;

                    rowNumber++;
                }

                var path = Directory.GetCurrentDirectory() + @"\temp_trade_export.xlsx";
                var fileInfo = new FileInfo(path);
                xlPackage.SaveAs(fileInfo);

                return fileInfo;
            }
        }
    }
}
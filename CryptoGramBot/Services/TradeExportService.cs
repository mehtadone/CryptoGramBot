using System.IO;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace CryptoGramBot.Services
{
    public class TradeExportService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<TradeExportService> _log;

        public TradeExportService(ILogger<TradeExportService> log, DatabaseService databaseService)
        {
            _log = log;
            _databaseService = databaseService;
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
    }
}
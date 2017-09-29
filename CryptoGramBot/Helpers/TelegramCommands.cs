using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Helpers
{
    public static class TelegramCommands
    {
        public static string BittrexBalanceInfo = "/btc_balance_bittrex";
        public static string BittrexTradeExportUpload = "/upload_bittrex_trades";
        public static string CoinigyAccountBalance = "/acc_n";
        public static string CoinigyAccountList = "/list_coinigy_accounts";
        public static string CoinigyTotalBalance = "/total_coinigy";
        public static string CommonExcel = "/trade_export";
        public static string CommonPairProfit = "/profit";
        public static string PoloniexBalanceInfo = "/btc_balance_poloniex";
    }
}
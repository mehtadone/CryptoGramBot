using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CryptoGramBot.Helpers
{
    public static class Helpers
    {
        public static decimal BalanceForAuthId(JObject jObject)
        {
            var data = jObject["data"];
            return data.Select(token => token["btc_balance"].ToString()).Aggregate<string, decimal>(0, (current, btcBalance) => current + decimal.Parse(btcBalance));
        }

        public static decimal GetLastBid(JObject jObject)
        {
            var data = jObject["data"];
            var stringBid = data[0]["bid"].ToString();
            return decimal.Parse(stringBid);
        }

        public static decimal TotalBtcBalance(JObject jObject)
        {
            decimal btcBalance = 0;
            btcBalance = jObject["data"]
                .Select(token => token["btc_balance"].ToString())
                .Select(decimal.Parse)
                .Aggregate(btcBalance, (current, d) => current + d);
            return btcBalance;
        }

        public static DateTime BiananceTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp / 1000).ToUniversalTime();
            return dtDateTime;
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return Convert.ToInt64((TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds);
        }

        public static string FormatCurrencyAmount(decimal amount, string ccy)
        {
            switch(ccy)
            {
                case "USD":
                    return string.Format("${0}", amount.ToString("###0.##"));

                case "EUR":
                    return string.Format("€{0}", amount.ToString("###0.##"));

                case "GBP":
                    return string.Format("£{0}", amount.ToString("###0.##"));

                case "JPY":
                    return string.Format("¥{0}", amount.ToString("###0.##"));

                case "KRW":
                    return string.Format("₩{0}", amount.ToString("###0.##"));

                default:
                    // Default formatting, e.g. 1234.56 CCY
                    return string.Format("{0} {1}", amount.ToString("###0.##"), ccy);
            }
        }

        public static bool CurrenciesAreEquivalent(string ccy1, string ccy2)
        {
            if (ccy1 == ccy2)
            {
                return true;
            }

            if ((ccy1 == "USD" && ccy2 == "USDT") || (ccy1 == "USDT" && ccy2 == "USD"))
            {
                return true;
            }

            return false;
        }
    }
}
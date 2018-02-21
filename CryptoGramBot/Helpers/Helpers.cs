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
    }
}
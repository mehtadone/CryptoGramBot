using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TeleCoinigy.Helpers
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
    }
}
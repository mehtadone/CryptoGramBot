using System.Linq;
using Newtonsoft.Json.Linq;

namespace TeleCoinigy.Helpers
{
    public static class Helpers
    {
        public static double TotalBtcBalance(JObject jObject)
        {
            double btcBalance = 0;
            btcBalance = jObject["data"]
                .Select(token => token["btc_balance"].ToString())
                .Select(double.Parse)
                .Aggregate(btcBalance, (current, d) => current + d);
            return btcBalance;
        }
    }
}
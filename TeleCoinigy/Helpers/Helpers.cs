using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TeleCoinigy.Helpers
{
    public static class Helpers
    {
        public static double BalanceForAuthId(JObject jObject)
        {
            var data = jObject["data"];
            return data.Select(token => token["btc_balance"].ToString()).Aggregate<string, double>(0, (current, btcBalance) => current + double.Parse(btcBalance));
        }

        public static double GetLastBid(JObject jObject)
        {
            var data = jObject["data"];
            var stringBid = data[0]["bid"].ToString();
            return double.Parse(stringBid);
        }

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
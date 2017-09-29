using Jojatekok.PoloniexAPI;
using Poloniex.General;
using Poloniex.LiveTools;
using Poloniex.MarketTools;
using Poloniex.TradingTools;
using Poloniex.WalletTools;

namespace Poloniex
{
    public sealed class PoloniexClient
    {
        /// <summary>Creates a new instance of Poloniex API .NET's client service.</summary>
        /// <param name="publicApiKey">Your public API key.</param>
        /// <param name="privateApiKey">Your private API key.</param>
        public PoloniexClient(string publicApiKey, string privateApiKey)
        {
            var apiWebClient = new ApiWebClient(Helper.ApiUrlHttpsBase);

            Authenticator = new Authenticator(apiWebClient, publicApiKey, privateApiKey);

            Markets = new Markets(apiWebClient);
            Trading = new Trading(apiWebClient);
            Wallet = new Wallet(apiWebClient);
            Live = new Live();
        }

        /// <summary>Creates a new, unauthorized instance of Poloniex API .NET's client service.</summary>
        public PoloniexClient() : this("", "")
        {
        }

        /// <summary>Represents the authenticator object of the client.</summary>
        public IAuthenticator Authenticator { get; }

        /// <summary>A class which represents live data fetched automatically from the server.</summary>
        public ILive Live { get; }

        /// <summary>A class which contains market tools for the client.</summary>
        public IMarkets Markets { get; }

        /// <summary>A class which contains trading tools for the client.</summary>
        public ITrading Trading { get; }

        /// <summary>A class which contains wallet tools for the client.</summary>
        public IWallet Wallet { get; }
    }
}
using System;

namespace Jojatekok.PoloniexAPI
{
    /// <summary>
    /// The <see cref="IPoloniexClient"/> interface abstracts the <see cref="PoloniexClient"/> class for use in dependency-injection scenarios.
    /// </summary>
    public interface IPoloniexClient : IDisposable
    {
        /// <summary>Represents the authenticator object of the client.</summary>
        IAuthenticator Authenticator { get; }

        /// <summary>A class which contains market tools for the client.</summary>
        IMarkets Markets { get; }

        /// <summary>A class which contains trading tools for the client.</summary>
        ITrading Trading { get; }

        /// <summary>A class which contains wallet tools for the client.</summary>
        IWallet Wallet { get; }
    }
}
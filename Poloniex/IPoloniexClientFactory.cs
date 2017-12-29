namespace Jojatekok.PoloniexAPI
{
    /// <summary>
    /// Defines methods used to create <see cref="PoloniexClientFactory"/>
    /// </summary>
    public interface IPoloniexClientFactory
    {
        /// <summary>
        /// Creates an anonymous <see cref="IPoloniexClient"/> that can be used to read market data.
        /// </summary>
        /// <returns>An initialized <see cref="IPoloniexClient"/> instance.</returns>
        IPoloniexClient CreateAnonymousClient();
        /// <summary>
        /// Creates an authenticated <see cref="IPoloniexClient"/> that can be used to query wallet balances and execute trades or transfers.
        /// </summary>
        /// <param name="publicKey">A <see cref="string"/> containing the public key used to authenticate with poloniex servers.</param>
        /// <param name="privateApiKey">A <see cref="string"/> containing the private api key used to authenticate with poloniex servers.</param>
        /// <returns>An initialized <see cref="IPoloniexClient"/> instance.</returns>
        IPoloniexClient CreateClient(string publicKey, string privateApiKey);
    }
}

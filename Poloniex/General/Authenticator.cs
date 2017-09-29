using Jojatekok.PoloniexAPI;

namespace Poloniex.General
{
    public class Authenticator : IAuthenticator
    {
        internal Authenticator(ApiWebClient apiWebClient, string publicKey, string privateKey) : this(apiWebClient)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
            apiWebClient.Authenticator = this;
        }

        internal Authenticator(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }
        private ApiWebClient ApiWebClient { get; set; }
    }
}
namespace Jojatekok.PoloniexAPI
{
    public class Authenticator : IAuthenticator
    {
        private ApiWebClient ApiWebClient { get; set; }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

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
    }
}

using Newtonsoft.Json;

namespace Poloniex.WalletTools
{
    public class GeneratedDepositAddress : IGeneratedDepositAddress
    {
        [JsonProperty("response")]
        public string Address { get; private set; }

        public bool IsGenerationSuccessful { get; private set; }

        [JsonProperty("success")]
        private byte IsGenerationSuccessfulInternal
        {
            set { IsGenerationSuccessful = value == 1; }
        }
    }
}
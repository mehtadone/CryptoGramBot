using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class GeneratedDepositAddress : IGeneratedDepositAddress
    {
        [JsonProperty("success")]
        private byte IsGenerationSuccessfulInternal {
            set { IsGenerationSuccessful = value == 1; }
        }
        public bool IsGenerationSuccessful { get; private set; }

        [JsonProperty("response")]
        public string Address { get; private set; }
    }
}

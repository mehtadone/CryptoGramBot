using Newtonsoft.Json;
using System.Net;

namespace Jojatekok.PoloniexAPI
{
    class JsonResponse<T>
    {
        [JsonProperty("status")]
        private string Status { get; set; }
        [JsonProperty("message")]
        private string Message { get; set; }

        private T _data;
        [JsonProperty("data")]
        internal T Data {
            get { return _data; }

            private set {
                CheckStatus();
                _data = value;
            }
        }

        internal void CheckStatus()
        {
            if (Status != "success") {
                if (string.IsNullOrWhiteSpace(Message)) throw new WebException("Could not parse data from the server.", WebExceptionStatus.UnknownError);
                throw new WebException("Could not parse data from the server: " + Message, WebExceptionStatus.UnknownError);
            }
        }
    }
}

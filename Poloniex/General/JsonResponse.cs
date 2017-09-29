using System.Net;
using Newtonsoft.Json;

namespace Poloniex.General
{
    internal class JsonResponse<T>
    {
        private T _data;

        [JsonProperty("data")]
        internal T Data
        {
            get => _data;

            private set
            {
                CheckStatus();
                _data = value;
            }
        }

        [JsonProperty("message")]
        private string Message { get; set; }

        [JsonProperty("status")]
        private string Status { get; set; }

        internal void CheckStatus()
        {
            if (Status != "success")
            {
                if (string.IsNullOrWhiteSpace(Message)) throw new WebException("Could not parse data from the server.", WebExceptionStatus.UnknownError);
                throw new WebException("Could not parse data from the server: " + Message, WebExceptionStatus.UnknownError);
            }
        }
    }
}
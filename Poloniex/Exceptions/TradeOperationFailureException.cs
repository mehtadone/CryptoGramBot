using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jojatekok.PoloniexAPI.Exceptions
{
    [Serializable]
    public class TradeOperationFailureException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public TradeOperationFailureException()
        {
        }

        public TradeOperationFailureException(string message) : base(message)
        {
        }

        public TradeOperationFailureException(string message, Exception inner) : base(message, inner)
        {
        }


        public TradeOperationFailureException(JObject response) : base(response.Value<string>("error") ?? response.Value<string>("message"))
        {
            Data["response"] = response.ToString(Formatting.None);
        }

        protected TradeOperationFailureException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string Response => Data["Response"] as string;
    }
}

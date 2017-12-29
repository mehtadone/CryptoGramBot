using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI.Exceptions
{
    [Serializable]
    public class ClientChannelHttpException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        /// <summary>
        /// Creates a new <see cref="ClientChannelHttpException"/>.
        /// </summary>
        public ClientChannelHttpException()
            : this(500, "Server Eror")
        {
        }
        /// <summary>
        /// Creates a new <see cref="ClientChannelHttpException"/>.
        /// </summary>
        /// <param name="errorCode">The <see cref="ErrorCode"/> value.</param>
        /// <param name="message">The <see cref="Exception.Message"/> value.</param>
        public ClientChannelHttpException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
        /// <summary>
        /// Creates a new <see cref="ClientChannelHttpException"/>.
        /// </summary>
        /// <param name="errorCode">The <see cref="ErrorCode"/> value.</param>
        /// <param name="message">The <see cref="Exception.Message"/> value.</param>
        /// <param name="inner">The <see cref="Exception.InnerException"/> value.</param>
        public ClientChannelHttpException(int errorCode, string message, Exception inner) : base(message, inner)
        {
            ErrorCode = errorCode;
        }
        /// <summary>
        /// Creates a new <see cref="ClientChannelHttpException"/>.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ClientChannelHttpException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
        /// <summary>
        /// Http Error Code associated with the channel failure.
        /// </summary>
        public int ErrorCode
        {
            get => (int) Data["HttpErr::ErrorCode"];
            set => Data["HttpErr::ErrorCode"] = value;
        }
        /// <summary>
        /// Tries to create a <see cref="ClientChannelHttpException"/> from <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Exception"/>.</param>
        /// <param name="exception">When successful, this is set to the newly created <see cref="ClientChannelHttpException"/></param>
        /// <returns><c>true</c> if a <see cref="ClientChannelHttpException"/> was able to be created, otherwise <c>false</c>.</returns>
        public static bool TryCreateFrom(Exception source, out ClientChannelHttpException exception)
        {
            exception = CreateFrom(source);
            return exception != null;
        }
        /// <summary>
        /// Create a <see cref="ClientChannelHttpException"/> from <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source <see cref="Exception"/>.</param>
        /// <returns>A <see cref='ClientChannelHttpException'/> created from <paramref name="source"/>, or <c>null</c> if the conversion is impossible.</returns>
        public static ClientChannelHttpException CreateFrom(Exception source)
        {
            // First try to cast directly
            var from = source as ClientChannelHttpException;
            if (from != null) return from;
            // Else try to parse exception text
            var match = HttpExceptionRegex.Match(source.Message);
            // Return null or new exception
            return !match.Success 
                ? default(ClientChannelHttpException) 
                : new ClientChannelHttpException(int.Parse(match.Groups["ErrorCode"].Value), match.Groups["Message"].Value, source);
        }


        //HTTP/1.1 502 Bad Gateway
        /// <summary>
        /// Regular expression used to parse exceptions.
        /// </summary>
        private static readonly Regex HttpExceptionRegex = new Regex(@"^HTTP\/\d+\.\d+\s+(?<ErrorCode>\d+)\s+(?<Message>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}

using System;

namespace CryptoGramBot.Services.Exchanges
{
    public class ErrorFromExchangeException : Exception
    {
        public ErrorFromExchangeException(string message) : base(message)
        {
        }
    }
}
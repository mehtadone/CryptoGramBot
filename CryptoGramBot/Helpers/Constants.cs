using System;
using System.IO;

namespace CryptoGramBot.Helpers
{
    public class Constants
    {
        public const string Bittrex = "Bittrex";
        public const string Poloniex = "Poloniex";
        public const string CoinigyAccountBalance = "Coinigy Account Balance";
        public const string TotalCoinigyBalance = "Total Coinigy Balance";
        public static readonly DateTime DateTimeUnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}
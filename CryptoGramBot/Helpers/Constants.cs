using System;
using System.IO;

namespace CryptoGramBot.Helpers
{
    public class Constants
    {
        public static readonly string Bittrex = "Bittrex";
        public static readonly string CoinigyBalance = "COINIGY_BALANCE_NAME";
        public static readonly string DatabaseName = Directory.GetCurrentDirectory() + "/database/cryptogrambot.db";
        public static readonly DateTime DateTimeUnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static readonly string Poloniex = "Poloniex";
    }
}
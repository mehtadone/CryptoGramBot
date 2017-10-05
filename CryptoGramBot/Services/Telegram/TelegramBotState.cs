namespace CryptoGramBot.Services.Telegram
{
    public static class BittrexFileUploadState
    {
        public static bool Waiting { get; set; }

        public static bool Reset()
        {
            Waiting = false;

            return true;
        }
    }

    public static class PairProfitState
    {
        public static string CurrencyPair { get; set; }
        public static bool WaitingForCurrency { get; set; }

        public static bool Reset()
        {
            CurrencyPair = string.Empty;
            WaitingForCurrency = false;

            return true;
        }
    }
}
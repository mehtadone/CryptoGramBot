namespace CryptoGramBot.Helpers
{
    public static class StringContants
    {
        public static string BittrexCommands = "\n<strong>Bittrex commands</strong>)\n";
        public static string BittrexMoreThan30Deposits = "30 or more bittrex deposits available to send. Will not send them to avoid spam.";

        public static string BittrexMoreThan30OpenOrders =
            "30 or more open bittrex orders available to send. Will not send them to avoid spam.";

        public static string BittrexMoreThan30Trades =
            "There are more than 30 bittrex trades to send. Not going to send them to avoid spamming you";

        public static string BittrexMoreThan30Withdrawals = "30 or more bittrex withdrawals available to send. Will not send them to avoid spam.";
        public static string CoinigyCommands = "\n<strong>Coinigy commands</strong>)\n";
        public static string CoinigyConnectedAccounts = "Connected accounts on Coinigy are:";
        public static string CommonCommands = "<strong>Common commands</strong>\n";
        public static string CouldNotProcessCommand = "Could not process your command. Check your logs";
        public static string CouldNotProcessFile = "Could not process file.";
        public static string CouldNotWorkOutPair = "Could not work out what the pair you typed was";
        public static string DidNotRecieveFile = "Did not receive a file";
        public static string Help = "<strong>Help</strong>\n\n";
        public static string No24HourOfData = "Could not calculate percentages. Probably because we don't have 24 hours of data yet";
        public static string PairProfitError = "Something went wrong. Probably because you entered in a dud currency or I have no trade details";
        public static string PleaseUploadBittrexFile = "Please upload bittrex trade export";
        public static string PoloCommands = "\n<strong>Poloniex commands</strong>)\n";
        public static string PoloniexMoreThan30Deposits = "30 or more poloniex deposits available to send. Will not send them to avoid spam.";

        public static string PoloniexMoreThan30OpenOrders =
            "30 or more open bittrex orders available to send. Will not send them to avoid spam.";

        public static string PoloniexMoreThan30Trades =
                                                    "There are more than 30 bittrex trades to send. Not going to send them to avoid spamming you";

        public static string PoloniexMoreThan30Withdrawals = "30 or more poloniex withdrawals available to send. Will not send them to avoid spam.";
        public static string PoloniexResetTrades = "I've reset your trades with trades from polo.";
        public static string Welcome = "<strong>Welcome to CryptoGramBot. Type /help for commands.</strong>\n";
        public static string WhatPairProfits = "What pair do you want to find your profits on? eg BTC-DOGE";
    }
}
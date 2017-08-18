using System;
using System.Threading.Tasks;
using TeleCoinigy.Configuration;
using TeleCoinigy.Database;

namespace TeleCoinigy.Services
{
    public class MainService
    {
        private readonly CoinigyConfig _coinigyConfig;
        private readonly CoinigyApiService _coinigyService;
        private readonly DatabaseService _databaseService;
        private readonly TelegramService _telegramService;

        public MainService(CoinigyConfig coinigyConfig, TelegramConfig telegramConfig)
        {
            _coinigyConfig = coinigyConfig;
            _telegramService = new TelegramService(telegramConfig);
            _databaseService = new DatabaseService();
            _coinigyService = new CoinigyApiService(coinigyConfig);
        }

        public async Task SendAccountInfo()
        {
            Console.WriteLine("Getting account details from Coinigy");
            var accounts = await _coinigyService.GetAccounts();
            Console.WriteLine("Sending account details to telegram");
            await _telegramService.SendAccountInfo(accounts);
            Console.WriteLine("Telegram message sent");
        }

        public async Task SendSpecificAccountDetails()
        {
            Console.WriteLine("Getting authId for Gunbot");
            string authID = _coinigyService.GetAuthIdFor(_coinigyConfig.SpecificAccountBalance);
            Console.WriteLine("AuthId for Gunbot is " + authID);
            Console.WriteLine("Getting BTC balance for Gunbot");
            var btcBalance = await _coinigyService.GetBtcBalance(authID);
            Console.WriteLine("Gunbot balance is " + btcBalance);
            Console.WriteLine("Getting previous Gunbot balance from DB");
            var previousGunbot = _databaseService.GetLastBalance(_coinigyConfig.SpecificAccountBalance);
            Console.WriteLine("Previous Gunbot balance from DB is " + previousGunbot);
            Console.WriteLine("Adding to database");
            _databaseService.AddBalance(btcBalance, _coinigyConfig.SpecificAccountBalance);
            Console.WriteLine("Added to database");
            Console.WriteLine("Sending telegram message");
            await _telegramService.SendBalanceUpdate(btcBalance, previousGunbot, _coinigyConfig.SpecificAccountBalance);
            Console.WriteLine("Sent telegram message");
            Console.WriteLine("Waiting until next run");
        }

        public async Task SendTotalBalanceUpdate()
        {
            Console.WriteLine("Getting balances for each account from Coinigy");
            await _coinigyService.SaveBalancesForEachAccount(_databaseService);
            Console.WriteLine("Getting total balance from Coinigy");
            double btcBalance = await _coinigyService.GetBtcBalance();
            Console.WriteLine("Balance from Coinigy is " + btcBalance + " BTC");
            Console.WriteLine("Getting last balance from database");
            double lastBalance = _databaseService.GetLastBalance(Constants.CoinigyBalance);
            Console.WriteLine("Last balance was " + lastBalance);
            Console.WriteLine("Adding to database");
            _databaseService.AddBalance(btcBalance, Constants.CoinigyBalance);
            Console.WriteLine("Added to database");
            Console.WriteLine("Sending telegram message");
            await _telegramService.SendBalanceUpdate(btcBalance, lastBalance);
            Console.WriteLine("Telegram message sent");
            Console.WriteLine("Waiting until next run");
        }
    }
}
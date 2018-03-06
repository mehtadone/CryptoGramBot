using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Commands
{
    public class SendBalanceInfoCommand : ICommand
    {
        public SendBalanceInfoCommand(
            BalanceInformation balanceInformation)
        {
            BalanceInformation = balanceInformation;
        }

        public BalanceInformation BalanceInformation { get; }
    }
}
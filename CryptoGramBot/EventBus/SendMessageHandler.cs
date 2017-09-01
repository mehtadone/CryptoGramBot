using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus
{
    public class SendMessageCommand : ICommand
    {
        private readonly string _message;

        public SendMessageCommand(string message)
        {
            _message = message;
        }
    }

    public class SendMessageHandler : ICommandHandler<SendMessageCommand>
    {
        public Task Handle(SendMessageCommand command)
        {
            return Task.FromResult(0);
        }
    }
}
using System;

namespace Jojatekok.PoloniexAPI
{
    public class TrollboxMessageEventArgs : EventArgs
    {
        public string SenderName { get; private set; }
        public uint? SenderReputation { get; private set; }
        public ulong MessageNumber { get; private set; }
        public string MessageText { get; private set; }

        internal TrollboxMessageEventArgs(string senderName, uint? senderReputation, ulong messageNumber, string messageText)
        {
            SenderName = senderName;
            SenderReputation = senderReputation;
            MessageNumber = messageNumber;
            MessageText = messageText;
        }
    }
}

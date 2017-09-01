using System;

namespace CryptoGramBot.Models
{
    public class LastChecked
    {
        public string Exchange { get; set; }
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
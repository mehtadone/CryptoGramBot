namespace CryptoGramBot.Models
{
    public class Currency
    {
        public string Base { get; set; }
        public string Terms { get; set; }

        public override string ToString()
        {
            return $"{Base}-{Terms}";
        }
    }
}
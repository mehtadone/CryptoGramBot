using System.ComponentModel.DataAnnotations;

namespace CryptoGramBot.Models
{
    public class CoinigyAccount
    {
        [Key]
        public string AuthId { get; set; }

        public string Name { get; set; }
    }
}
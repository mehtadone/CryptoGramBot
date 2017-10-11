using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
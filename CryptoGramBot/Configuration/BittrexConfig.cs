using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Configuration
{
    public class BittrexConfig
    {
        public bool Enabled { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
    }
}
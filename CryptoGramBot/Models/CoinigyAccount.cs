using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Models
{
    public class CoinigyAccount
    {
        [Key]
        public string AuthId { get; set; }

        public string Name { get; set; }
    }
}
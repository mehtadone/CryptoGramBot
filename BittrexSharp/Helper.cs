using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BittrexSharp
{
    public class Helper
    {
        public static string GetSourceCurrencyFromMarketName(string marketName) => marketName.Split('-').First();
        public static string GetTargetCurrencyFromMarketName(string marketName) => marketName.Split('-').Last();
    }
}

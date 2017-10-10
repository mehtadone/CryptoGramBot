using System;
using System.Collections.Generic;
using System.Text;

namespace BittrexSharp.Domain
{
    public static class OrderType
    {
        public static string Buy { get; } = "buy";
        public static string Sell { get; } = "sell";
        public static string Both { get; } = "both";
    }
}

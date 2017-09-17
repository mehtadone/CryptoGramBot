using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    internal class ApiCallResponse<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
        public bool Success { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bittrex
{
    class ApiCallResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T result { get; set; }
    }
}

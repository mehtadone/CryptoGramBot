using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jojatekok.PoloniexAPI
{
    public class PoloniexClientFactory
        : IPoloniexClientFactory
    {
        public virtual IPoloniexClient CreateAnonymousClient()
        {
            return new PoloniexClient();
        }

        public virtual IPoloniexClient CreateClient(string publicKey, string privateApiKey)
        {
            return new PoloniexClient(publicKey, privateApiKey);
        }
    }
}

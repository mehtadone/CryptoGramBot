using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudFlareUtilities;

namespace Bittrex
{
    public class ApiCall
    {
        private readonly bool _simulate;

        public ApiCall(bool simulate)
        {
            this._simulate = simulate;
        }

        public async Task<T> CallWithJsonResponse<T>(string uri, bool hasEffects, params Tuple<string, string>[] headers)
        {
            if (_simulate && hasEffects)
            {
                Debug.WriteLine("(simulated)" + GetCallDetails(uri));
                return default(T);
            }

            Debug.WriteLine(GetCallDetails(uri));

            try
            {
                // Create the clearance handler.
                var handler = new ClearanceHandler
                {
                    MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
                };

                var client = new HttpClient(handler);

                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Item1, header.Item2);
                }

                var content = await client.GetStringAsync(uri);

                var jsonResponse = JsonConvert.DeserializeObject<ApiCallResponse<T>>(content);

                if (jsonResponse.Success)
                {
                    return jsonResponse.Result;
                }
                else
                {
                    throw new Exception(jsonResponse.Message.ToString() + "Call Details=" + GetCallDetails(uri));
                }
            }
            catch (AggregateException ex) when (ex.InnerException is CloudFlareClearanceException)
            {
                // After all retries, clearance still failed.
                throw new Exception(ex.Message);
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                // Looks like we ran into a timeout. Too many clearance attempts?
                // Maybe you should increase client.Timeout as each attempt will take about five seconds.
                throw new Exception(ex.Message);
            }
        }

        private static string GetCallDetails(string uri)
        {
            StringBuilder sb = new StringBuilder();
            var u = new Uri(uri);
            sb.Append(u.AbsolutePath);
            if (u.Query.StartsWith("?"))
            {
                var queryParameters = u.Query.Substring(1).Split('&');
                foreach (var p in queryParameters)
                {
                    if (!(p.ToLower().StartsWith("api") || p.ToLower().StartsWith("nonce")))
                    {
                        var kv = p.Split('=');
                        if (kv.Length == 2)
                        {
                            if (sb.Length != 0)
                            {
                                sb.Append(", ");
                            }

                            sb.Append(kv[0]).Append(" = ").Append(kv[1]);
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Net;
using System.Threading.Tasks;

namespace Elmah.Io.Client
{
    static class IWebClientExtensions
    {
        public static Task<string> Post(this IWebClient client, Uri address, string data)
        {
            return Post(client, null, address, data);
        }

        public static Task<string> Post(this IWebClient client, WebHeaderCollection headers, Uri address, string data)
        {
            if (client == null) throw new ArgumentNullException("client");
            return client.Post(headers, address, data, (responseHeaders, r) => responseHeaders["Location"]);
        }

        public static Task<string> Get(this IWebClient client, Uri address)
        {
            return Get(client, null, address);
        }

        public static Task<string> Get(this IWebClient client, WebHeaderCollection headers, Uri address)
        {
            if (client == null) throw new ArgumentNullException("client");
            return client.Get(headers, address, (_, r) => r);
        }
    }
}

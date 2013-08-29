using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Elmah.Io
{
    static class IWebClientExtensions
    {
        public static string Post(this IWebClient client, Uri address, string data)
        {
            return Post(client, null, address, data);
        }

        public static string Post(this IWebClient client, WebHeaderCollection headers, Uri address, string data)
        {
            if (client == null) throw new ArgumentNullException("client");
            return client.Post(headers, address, data, (_, r) => r);
        }

        public static string Get(this IWebClient client, Uri address)
        {
            return Get(client, null, address);
        }

        public static string Get(this IWebClient client, WebHeaderCollection headers, Uri address)
        {
            if (client == null) throw new ArgumentNullException("client");
            return client.Get(headers, address, (_, r) => r);
        }
    }
}

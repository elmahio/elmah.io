using System;
using System.Net;

namespace Elmah.Io
{
    public interface IWebClient
    {
        T Post<T>(WebHeaderCollection headers, Uri address, string data, Func<WebHeaderCollection, string, T> resultor);
        T Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor);
    }
                                        // ReSharper disable InconsistentNaming
    static class IWebClientExtensions   // ReSharper restore InconsistentNaming
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
using System;
using System.Net;

namespace Elmah.Io
{
    internal class DotNetWebClientProxy : IWebClient
    {
        public T Post<T>(WebHeaderCollection headers, Uri address, string data, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");
            
            using (var webClient = new WebClient())
            {
                if (headers != null)
                {
                    webClient.Headers.Add(headers);
                }

                return resultor(webClient.ResponseHeaders, webClient.UploadString(address, data));
            }
        }

        public T Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");

            using (var webClient = new WebClient())
            {
                if (headers != null)
                {
                    webClient.Headers.Add(headers);
                }

                return resultor(webClient.ResponseHeaders, webClient.DownloadString(address));
            }
        }
    }
}
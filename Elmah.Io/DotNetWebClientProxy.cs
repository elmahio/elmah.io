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
            
            using (var wc = new WebClient())
            {
                if (headers != null) 
                    wc.Headers.Add(headers);
                return resultor(wc.ResponseHeaders, wc.UploadString(address, data));
            }
        }

        public T Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");

            using (var wc = new WebClient())
            {
                if (headers != null) 
                    wc.Headers.Add(headers);
                return resultor(wc.ResponseHeaders, wc.DownloadString(address));
            }
        }
    }
}
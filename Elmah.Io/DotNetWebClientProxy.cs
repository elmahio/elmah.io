using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Elmah.Io
{
    internal class DotNetWebClientProxy : IWebClient
    {
        public Task<T> Post<T>(WebHeaderCollection headers, Uri address, string data, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");
            
            return Transact(headers, webClient => webClient.UploadStringAsTask(address, data), resultor);
        }

        public Task<T> Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor)
        {
            if (address == null) throw new ArgumentNullException("address");
            if (resultor == null) throw new ArgumentNullException("resultor");

            return Transact(headers, webClient => webClient.DownloadStringAsTask(address), resultor);
        }

        static Task<TResult> Transact<T, TResult>(NameValueCollection headers, Func<WebClient, Task<T>> transactor, Func<WebHeaderCollection, T, TResult> resultor)
        {
            Debug.Assert(transactor != null);
            Debug.Assert(resultor != null);

            var webClient = new WebClient();
            IDisposable disposable = webClient;
            try
            {
                if (headers != null)
                {
                    webClient.Headers.Add(headers);
                }

                var task = transactor(webClient).ContinueWith(t =>
                {
                    try
                    {
                        return resultor(webClient.ResponseHeaders, t.Result);
                    }
                    finally 
                    {
                        webClient.Dispose();
                    }
                },
                TaskContinuationOptions.ExecuteSynchronously);

                disposable = null;
                return task;
            }
            finally
            {
                if (disposable != null) disposable.Dispose();
            }
        }
    }
}
using System;
using System.Net;

namespace Elmah.Io
{
    internal class DotNetWebClientProxy : IWebClient
    {
        private readonly WebClient _webClient;

        public DotNetWebClientProxy()
        {
            _webClient = new WebClient();
        }

        public WebHeaderCollection Headers
        {
            get { return _webClient.Headers; }
            set { _webClient.Headers = value; }
        }

        public string Get(Uri address)
        {
            return _webClient.DownloadString(address);
        }

        public string Post(Uri address, string data)
        {
            return _webClient.UploadString(address, data);
        }

        public void Dispose()
        {
            if (_webClient != null)
            {
                _webClient.Dispose();
            }
        }
    }
}
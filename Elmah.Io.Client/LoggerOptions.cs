using System;

namespace Elmah.Io.Client
{
    public class LoggerOptions
    {
        public const string ElmahIoApiUrl = "https://elmah.io/";

        public LoggerOptions()
        {
            Url = new Uri(ElmahIoApiUrl);
            WebClient = new DotNetWebClientProxy();
        }

        public Uri Url { get; set; }

        public bool Durable { get; set; }

        public string FailedRequestPath { get; set; }

        public IWebClient WebClient { get; set; }
    }
}
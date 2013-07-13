using System;
using System.Net;

namespace Elmah.Io
{
    public interface IWebClient : IDisposable
    {
        WebHeaderCollection Headers { get; set; }

        string Post(Uri address, string data);

        string Get(Uri address);
    }
}
using System;
using System.Net;

namespace Elmah.Io
{
    public interface IWebClient
    {
        T Post<T>(WebHeaderCollection headers, Uri address, string data, Func<WebHeaderCollection, string, T> resultor);
        T Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor);
    }
}
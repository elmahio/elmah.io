using System;
using System.Net;
using System.Threading.Tasks;

namespace Elmah.Io.Client
{
    public interface IWebClient
    {
        Task<T> Post<T>(WebHeaderCollection headers, Uri address, string data, Func<WebHeaderCollection, string, T> resultor);
        Task<T> Get<T>(WebHeaderCollection headers, Uri address, Func<WebHeaderCollection, string, T> resultor);
    }
}
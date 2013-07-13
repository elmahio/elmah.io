using System;

namespace Elmah.Io
{
    public interface IWebClientFactory
    {
        IWebClient Create();
    }
}
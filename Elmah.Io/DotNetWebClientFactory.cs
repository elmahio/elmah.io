namespace Elmah.Io
{
    internal class DotNetWebClientFactory : IWebClientFactory
    {
        public IWebClient Create()
        {
            return new DotNetWebClientProxy();
        }
    }
}
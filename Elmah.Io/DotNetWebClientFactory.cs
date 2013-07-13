namespace Elmah.Io
{
    public class DotNetWebClientFactory : IWebClientFactory
    {
        public IWebClient Create()
        {
            return new DotNetWebClientProxy();
        }
    }
}
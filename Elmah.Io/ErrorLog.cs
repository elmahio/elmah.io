using Mannex;
using Mannex.Web;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog
    {
        private readonly string _logId;
        private readonly Uri _url = new Uri("http://elmahio.azurewebsites.net/");
        private readonly IWebClientFactory _webClientFactory;

        public ErrorLog(IDictionary config) : this(config, new DotNetWebClientFactory())
        {
        }

        public ErrorLog(IDictionary config, IWebClientFactory webClientFactory)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (!config.Contains("LogId"))
            {
                throw new ApplicationException("Missing LogId. Please specify a LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
            }

            Guid result;
            if (!Guid.TryParse(config["LogId"].ToString(), out result))
            {
                throw new ApplicationException("Invalid LogId. Please specify a valid LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
            }

            _logId = result.ToString();

            if (config.Contains("Url"))
            {
                Uri uri;
                if (!Uri.TryCreate(config["Url"].ToString(), UriKind.Absolute, out uri))
                {
                    throw new ApplicationException("Invalid URL. Please specify a valid absolute url. In fact you don't even need to specify an url, which will make the error logger use the elmah.io backend.");
                }

                _url = new Uri(config["Url"].ToString());
            }

            _webClientFactory = webClientFactory;
        }

        Uri ApiUrl(NameValueCollection query = null)
        {
            var q = new NameValueCollection
            {
                { "logId", _logId }, 
                query ?? new NameValueCollection()
            };
            return new Uri(_url, "api/logs" + q.ToQueryString());
        }

        public override string Log(Error error)
        {
            using (var webClient = _webClientFactory.Create())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var xml = ErrorXml.EncodeString(error);

                return webClient.Post(ApiUrl(), "=" + HttpUtility.UrlEncode(xml));
            }
        }

        public override ErrorLogEntry GetError(string id)
        {
            string response;
            using (var webClient = _webClientFactory.Create())
            {
                response = webClient.Get(ApiUrl(new NameValueCollection { { "id", id } }));
            }

            dynamic d = JsonConvert.DeserializeObject(response);
            return MapErrorLogEntry(d.Id, d.ErrorXml);
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            var url = ApiUrl(new NameValueCollection
            {
                { "pageindex", pageIndex.ToInvariantString() }, 
                { "pagesize", pageSize.ToInvariantString() }, 
            });
            string response;
            using (var webClient = _webClientFactory.Create())
            {
                response = webClient.Get(url);
            }

            dynamic d = JsonConvert.DeserializeObject(response);
            foreach (dynamic error in d)
            {
                errorEntryList.Add(MapErrorLogEntry(error.Id, error.ErrorXml));
            }

            return errorEntryList.Count;
        }

        private ErrorLogEntry MapErrorLogEntry(dynamic id, dynamic xml)
        {
            return new ErrorLogEntry(this, (string)id, ErrorXml.DecodeString((string)xml));
        }
    }
}
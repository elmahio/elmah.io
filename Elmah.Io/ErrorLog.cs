using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using Mannex;
using Mannex.Web;
using Newtonsoft.Json;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog
    {
        private readonly string _logId;
        private readonly Uri _url = new Uri("http://elmahio.azurewebsites.net/");
        private readonly IWebClient _webClient;

        public ErrorLog(IDictionary config) : this(config, new DotNetWebClientProxy())
        {
        }

        public ErrorLog(IDictionary config, IWebClient webClient)
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

            _webClient = webClient;
        }

        public override string Log(Error error)
        {
            var headers = new WebHeaderCollection { { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded" } };
            var xml = ErrorXml.EncodeString(error);
            return _webClient.Post(headers, ApiUrl(), "=" + HttpUtility.UrlEncode(xml));
        }

        public override ErrorLogEntry GetError(string id)
        {
            var response = _webClient.Get(ApiUrl(new NameValueCollection { { "id", id } }));
            dynamic error = JsonConvert.DeserializeObject(response);
            return MapErrorLogEntry((string) error.Id, (string) error.ErrorXml);
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            var url = ApiUrl(new NameValueCollection
            {
                { "pageindex", pageIndex.ToInvariantString() }, 
                { "pagesize", pageSize.ToInvariantString() }, 
            });
            
            var response = _webClient.Get(url);

            dynamic d = JsonConvert.DeserializeObject(response);

            var entries = from dynamic e in (IEnumerable) d.Errors
                          select MapErrorLogEntry((string) e.Id, 
                                                  (string) e.ErrorXml);

            foreach (var entry in entries)
            {
                errorEntryList.Add(entry);
            }

            return d.Total;
        }

        private ErrorLogEntry MapErrorLogEntry(string id, string xml)
        {
            return new ErrorLogEntry(this, id, ErrorXml.DecodeString(xml));
        }

        Uri ApiUrl(NameValueCollection query = null)
        {
            var q = new NameValueCollection
            {
                { "logId", _logId }, 
                query ?? new NameValueCollection()
            };
            return new Uri(_url, "api/logs2" + q.ToQueryString());
        }
    }
}
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Mannex;
using Mannex.Threading.Tasks;
using Mannex.Web;
using Newtonsoft.Json;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog, IErrorLog
    {
        private readonly string _logId;
        private readonly Uri _url = new Uri("http://elmahio.azurewebsites.net/");
        private readonly IWebClient _webClient;

        public ErrorLog(Guid logId)
        {
            _logId = logId.ToString();
            _webClient = new DotNetWebClientProxy();
        }

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
            return EndLog(BeginLog(error, null, null));
        }

        public override IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState)
        {
            var headers = new WebHeaderCollection { { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded" } };
            var xml = ErrorXml.EncodeString(error);
            return _webClient.Post(headers, ApiUrl(), "=" + HttpUtility.UrlEncode(xml))
                             .Apmize(asyncCallback, asyncState);
        }

        public override string EndLog(IAsyncResult asyncResult)
        {
            return EndImpl<string>(asyncResult);
        }

        public override IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return _webClient.Get(ApiUrl(new NameValueCollection { { "id", id } }))
                             .ContinueWith(t =>
                             {
                                 dynamic error = JsonConvert.DeserializeObject(t.Result);
                                 return MapErrorLogEntry((string) error.Id, (string) error.ErrorXml);
                             })
                             .Apmize(asyncCallback, asyncState);
        }

        public override ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            return EndImpl<ErrorLogEntry>(asyncResult);
        }

        public override ErrorLogEntry GetError(string id)
        {
            return EndGetError(BeginGetError(id, null, null));
        }

        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            var url = ApiUrl(new NameValueCollection
            {
                { "pageindex", pageIndex.ToInvariantString() }, 
                { "pagesize", pageSize.ToInvariantString() }, 
            });

            var task = _webClient.Get(url).ContinueWith(t =>
            {
                dynamic d = JsonConvert.DeserializeObject(t.Result);

                var entries = from dynamic e in (IEnumerable) d.Errors
                    select MapErrorLogEntry((string) e.Id,
                        (string) e.ErrorXml);

                foreach (var entry in entries)
                {
                    errorEntryList.Add(entry);
                }

                return (int) d.Total;
            });

            return task.Apmize(asyncCallback, asyncState);
        }

        public override int EndGetErrors(IAsyncResult asyncResult)
        {
            return EndImpl<int>(asyncResult);
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            return EndGetErrors(BeginGetErrors(pageIndex, pageSize, errorEntryList, null, null));
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

        static T EndImpl<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null) throw new ArgumentNullException("asyncResult");
            var task = asyncResult as Task<T>;
            if (task == null) throw new ArgumentException(null, "asyncResult");
            return task.Result;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
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
        private readonly Uri _url = new Uri("https://elmah.io/");
        private readonly IWebClient _webClient;

        public ErrorLog(Guid logId)
        {
            _logId = logId.ToString();
            _webClient = new DotNetWebClientProxy();
        }

        public ErrorLog(IDictionary config) : this(config, new DotNetWebClientProxy())
        {
        }

        /// <summary>
        /// ELMAH doesn't use this constructor and it is only published in order for you to create
        /// a new error logger using a custom implementation of IWebClient. If you do so, please
        /// identify yourself using an appropriate user agent.
        /// </summary>
        public ErrorLog(IDictionary config, IWebClient webClient)
        {
            if (config == null) 
            {
                throw new ArgumentNullException("config");
            }

            if (!config.Contains("LogId") && !config.Contains("LogIdKey"))
            {
                throw new ApplicationException("Missing LogId or LogIdKey. Please specify a LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" /> or using AppSettings: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogIdKey=\"MyAppSettingsKey\" /> where MyAppSettingsKey points to a AppSettings with the key 'MyAPpSettingsKey' and value equal LogId.");
            }

            _logId = ResolveLogId(config);
            _url = ResolveUrl(config);

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
                             .ContinueWith(t =>
                             {
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     return null;
                                 }

                                 dynamic d = JsonConvert.DeserializeObject(t.Result);
                                 return (string)d.Id;
                             })
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
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     return null;
                                 }
                             
                                 dynamic error = JsonConvert.DeserializeObject(t.Result);
                                 return MapErrorLogEntry((string)error.Id, (string)error.ErrorXml);
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
                if (t.Status != TaskStatus.RanToCompletion)
                {
                    return 0;
                }

                dynamic d = JsonConvert.DeserializeObject(t.Result);

                var entries = 
                    from dynamic e in (IEnumerable)d.Errors
                    select MapErrorLogEntry((string)e.Id, (string)e.ErrorXml);

                foreach (var entry in entries)
                {
                    errorEntryList.Add(entry);
                }

                return (int)d.Total;
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

        private Uri ApiUrl(NameValueCollection query = null)
        {
            var q = new NameValueCollection
            {
                { "logId", _logId }, 
                query ?? new NameValueCollection()
            };
            return new Uri(_url, "api/errors" + q.ToQueryString());
        }

        private T EndImpl<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            var task = asyncResult as Task<T>;
            if (task == null)
            {
                throw new ArgumentException(null, "asyncResult");
            }

            return task.Result;
        }

        private Uri ResolveUrl(IDictionary config)
        {
            if (!config.Contains("Url"))
            {
                return _url;
            }

            Uri uri;
            if (!Uri.TryCreate(config["Url"].ToString(), UriKind.Absolute, out uri))
            {
                throw new ApplicationException(
                    "Invalid URL. Please specify a valid absolute url. In fact you don't even need to specify an url, which will make the error logger use the elmah.io backend.");
            }

            return new Uri(config["Url"].ToString());
        }

        private string ResolveLogId(IDictionary config)
        {
            if (config.Contains("LogId"))
            {
                Guid result;
                if (!Guid.TryParse(config["LogId"].ToString(), out result))
                {
                    throw new ApplicationException(
                        "Invalid LogId. Please specify a valid LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
                }

                return result.ToString();
            }
            else
            {
                var appSettingsKey = config["LogIdKey"].ToString();
                var value = ConfigurationManager.AppSettings.Get(appSettingsKey);
                if (value == null)
                {
                    throw new ApplicationException(
                        "You are trying to reference a AppSetting which is not found (key = '" + appSettingsKey + "'");
                }

                Guid result;
                if (!Guid.TryParse(value, out result))
                {
                    throw new ApplicationException(
                        "Invalid LogId. Please specify a valid LogId in your web.config like this: <appSettings><add key=\""
                        + appSettingsKey + "\" value=\"98895825-2516-43DE-B514-FFB39EA89A65\" />");
                }

                return result.ToString();
            }
        }
    }
}
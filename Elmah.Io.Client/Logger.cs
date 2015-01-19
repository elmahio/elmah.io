using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mannex;
using Mannex.Threading.Tasks;
using Newtonsoft.Json;
using Mannex.Web;
using Newtonsoft.Json.Serialization;

namespace Elmah.Io.Client
{
    public class Logger : ILogger
    {
        private readonly Guid _logId;
        private readonly Uri _url = new Uri("https://elmah.io/");
        private readonly IWebClient _webClient;

        public Logger(Guid logId) : this(logId, null)
        {
        }

        public Logger(Guid logId, Uri url) : this(logId, url, new DotNetWebClientProxy())
        {
        }

        internal Logger(Guid logId, Uri url, IWebClient webClient)
        {
            _logId = logId;
            if (url != null) _url = url;
            _webClient = webClient;
        }

        public static Logger Create(Guid logId)
        {
            return new Logger(logId);
        }

        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Verbose(null, messageTemplate, propertyValues);
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Verbose, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            Debug(null, messageTemplate, propertyValues);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Debug, messageTemplate, propertyValues);
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            Information(null, messageTemplate, propertyValues);
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Information, messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            Warning(null, messageTemplate, propertyValues);
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Warning, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Error(null, messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Error, messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Fatal(null, messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Fatal, messageTemplate, propertyValues);
        }

        void Log(Exception exception, Severity severity, string messageTemplate, params object[] propertyValues)
        {
            var message = new Message(string.Format(messageTemplate, propertyValues)) { Severity = severity };
            if (exception != null)
            {
                message.Detail = exception.ToString();
            }

            Log(message);
        }

        public string Log(Message message)
        {
            return EndLog(BeginLog(message, null, null));
        }

        public IAsyncResult BeginLog(Message message, AsyncCallback asyncCallback, object asyncState)
        {
            var headers = new WebHeaderCollection { { HttpRequestHeader.ContentType, "application/json" } };

            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

            return _webClient.Post(headers, ApiUrl(), json)
                             .ContinueWith(t =>
                             {
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     return null;
                                 }

                                 Uri location;
                                 if (!Uri.TryCreate(t.Result, UriKind.Absolute, out location))
                                 {
                                     return null;
                                 }

                                 return (location.Query.TrimStart('?').Split('&').Select(parameter => parameter.Split('='))
                                     .Where(parameterSplitted => parameterSplitted.Length == 2 && parameterSplitted[0] == "id")
                                     .Select(parameterSplitted => parameterSplitted[1]))
                                     .FirstOrDefault();
                             })
                             .Apmize(asyncCallback, asyncState);
        }

        public string EndLog(IAsyncResult asyncResult)
        {
            return EndImpl<string>(asyncResult);
        }

        public Message GetMessage(string id)
        {
            return EndGetMessage(BeginGetMessage(id, null, null));
        }

        public IAsyncResult BeginGetMessage(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return _webClient.Get(ApiUrl(new NameValueCollection { { "id", id } }))
                             .ContinueWith(t =>
                             {
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     return null;
                                 }

                                 var message = JsonConvert.DeserializeObject<Message>(t.Result);
                                 return message;
                             })
                             .Apmize(asyncCallback, asyncState);
        }

        public Message EndGetMessage(IAsyncResult asyncResult)
        {
            return EndImpl<Message>(asyncResult);
        }

        public MessagesResult GetMessages(int pageIndex, int pageSize)
        {
            return EndGetMessages(BeginGetMessages(pageIndex, pageSize, null, null));
        }

        public IAsyncResult BeginGetMessages(int pageIndex, int pageSize, AsyncCallback asyncCallback, object asyncState)
        {
            var url = ApiUrl(new NameValueCollection
            {
                { "pageindex", pageIndex.ToInvariantString() }, 
                { "pagesize", pageSize.ToInvariantString() }, 
            });

            return _webClient.Get(url).ContinueWith(t =>
            {
                if (t.Status != TaskStatus.RanToCompletion)
                {
                    return null;
                }

                var messagesResult = JsonConvert.DeserializeObject<MessagesResult>(t.Result);

                return messagesResult;
            }).Apmize(asyncCallback, asyncState);
        }

        public MessagesResult EndGetMessages(IAsyncResult asyncResult)
        {
            return EndImpl<MessagesResult>(asyncResult);
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

        private Uri ApiUrl(NameValueCollection query = null)
        {
            var q = new NameValueCollection
            {
                { "logId", _logId.ToString() }, 
                query ?? new NameValueCollection()
            };
            return new Uri(_url, "api/v2/messages" + q.ToQueryString());
        }
    }
}
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Mannex;
using Mannex.Threading.Tasks;
using Mannex.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Elmah.Io.Client
{
    public class Logger : ILogger
    {
        private readonly Guid _logId;
        private readonly Uri _url = new Uri("https://elmah.io/");
        private readonly IWebClient _webClient;

        /// <summary>
        /// By subscribing to the OnMessage event, you can hook into the pipeline of logging a message to elmah.io.
        /// The event is triggered just before calling elmah.io's API.
        /// </summary>
        public static event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// By subscribing to the OnMessageFail event, you can get a call if an error happened while logging a message
        /// to elmah.io. In this case you should do something to log the message elsewhere.
        /// </summary>
        public static event EventHandler<FailEventArgs> OnMessageFail;

        /// <summary>
        /// Creates a new logger with the specified log ID. The logger is configured to use elmah.io's API.
        /// This is probably the constructor you want to use 99 % of the times.
        /// </summary>
        public Logger(Guid logId) : this(logId, null)
        {
        }

        /// <summary>
        /// Creates a new logger with the specified log ID and URL. This constructor is primarily ment for test
        /// purposes or if you are using a version of the elmah.io API not at its official location.
        /// </summary>
        public Logger(Guid logId, Uri url) : this(logId, url, new DotNetWebClientProxy())
        {
        }

        internal Logger(Guid logId, Uri url, IWebClient webClient)
        {
            _logId = logId;
            if (url != null) _url = url;
            _webClient = webClient;
        }

        /// <summary>
        /// Creates a instance of the logger. Do exactly the same as calling the constructor with a Guid parameter.
        /// </summary>
        public static Logger Create(Guid logId)
        {
            return new Logger(logId);
        }

        /// <summary>
        /// Write a log message with the Verbose severity.
        /// </summary>
        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            Verbose(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Verbose severity and associated exception.
        /// </summary>
        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Verbose, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Debug severity.
        /// </summary>
        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            Debug(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Debug severity and associated exception.
        /// </summary>
        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Debug, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Information severity.
        /// </summary>
        public void Information(string messageTemplate, params object[] propertyValues)
        {
            Information(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Information severity and associated exception.
        /// </summary>
        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Information, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Warning severity.
        /// </summary>
        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            Warning(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Warning severity and associated exception.
        /// </summary>
        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Warning, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Error severity.
        /// </summary>
        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Error(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Error severity and associated exception.
        /// </summary>
        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Log(exception, Severity.Error, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Fatal severity.
        /// </summary>
        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Fatal(null, messageTemplate, propertyValues);
        }

        /// <summary>
        /// Write a log message with the Verbose Fatal and associated exception.
        /// </summary>
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
                message.Data = exception.ToDataList();
            }

            Log(message);
        }

        /// <summary>
        /// Write a log message using the specified Message. The message encapsulates the data included in properties.
        /// </summary>
        public string Log(Message message)
        {
            return EndLog(BeginLog(message, null, null));
        }

        /// <summary>
        /// Async write a log message using the specified Message. The message encapsulates the data included in properties.
        /// </summary>
        public IAsyncResult BeginLog(Message message, AsyncCallback asyncCallback, object asyncState)
        {
            if (OnMessage != null) OnMessage(this, new MessageEventArgs(message));

            var headers = new WebHeaderCollection { { HttpRequestHeader.ContentType, "application/json" } };

            var json = JsonConvert.SerializeObject(message, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

            return _webClient.Post(headers, ApiUrl(), json)
                             .ContinueWith(t =>
                             {
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     if (OnMessageFail != null) OnMessageFail(this, new FailEventArgs(message, t.Exception));
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

        /// <summary>
        /// Async end of writing a log message.
        /// </summary>
        public string EndLog(IAsyncResult asyncResult)
        {
            return EndImpl<string>(asyncResult);
        }

        /// <summary>
        /// Gets a log message by its ID.
        /// </summary>
        public Message GetMessage(string id)
        {
            return EndGetMessage(BeginGetMessage(id, null, null));
        }

        /// <summary>
        /// Async gets a log message by its ID.
        /// </summary>
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

        /// <summary>
        /// Async end of get log message.
        /// </summary>
        public Message EndGetMessage(IAsyncResult asyncResult)
        {
            return EndImpl<Message>(asyncResult);
        }

        /// <summary>
        /// Get all messages in the specified page and in the page size.
        /// </summary>
        public MessagesResult GetMessages(int pageIndex, int pageSize)
        {
            return EndGetMessages(BeginGetMessages(pageIndex, pageSize, null, null));
        }

        /// <summary>
        /// Async get all messages in the specified page and in the page size.
        /// </summary>
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

        /// <summary>
        /// Async end of getting all messages.
        /// </summary>
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
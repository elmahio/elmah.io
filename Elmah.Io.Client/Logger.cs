using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Mannex;
using Mannex.Threading.Tasks;
using Mannex.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Elmah.Io.Client
{
    /// <summary>
    /// Implementation of the ILogger interface for logging messages to elmah.io's API.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly Guid _logId;

        public Guid LogId { get { return _logId; } }
        [Obsolete("Use Options.Url")]
        public Uri Url { get { return Options.Url; } }
        public LoggerOptions Options { get; set; }

        public event EventHandler<MessageEventArgs> OnMessage;

        public event EventHandler<FailEventArgs> OnMessageFail;

        private bool _handleFailingMessages;

        /// <summary>
        /// Creates a new logger with the specified log ID. The logger is configured to use elmah.io's API.
        /// This is probably the constructor you want to use 99 % of the times.
        /// </summary>
        public Logger(Guid logId) : this(logId, new LoggerOptions())
        {
        }

        /// <summary>
        /// Creates a new logger with the specified log ID and URL. This constructor is primarily ment for test
        /// purposes or if you are using a version of the elmah.io API not at its official location.
        /// </summary>
        [Obsolete("To create a new logger, use LoggerConfiguration or the constructor with a LoggerOptions parameter")]
        public Logger(Guid logId, Uri url) : this(logId, new LoggerOptions {Url = url})
        {
        }

        public Logger(Guid logId, LoggerOptions options)
        {
            if (logId == Guid.Empty) throw new ArgumentException("Set logId to a valid GUID");
            if (options == null) throw new ArgumentException("Set options to a new LoggerOptions object");
            if (options.Url == null) options.Url = new Uri(LoggerOptions.ElmahIoApiUrl);
            if (options.WebClient == null) options.WebClient = new DotNetWebClientProxy();
            if (!string.IsNullOrWhiteSpace(options.FailedRequestPath) && !Directory.Exists(options.FailedRequestPath)) throw new ArgumentException("Please specify an existing directory");
            if (options.Durable && string.IsNullOrWhiteSpace(options.FailedRequestPath)) throw new ArgumentException("When running in Durable mode remember to set FailedRequestPath");

            _logId = logId;
            Options = options;

            if (Options.Durable)
            {
                var timer = new Timer(5000);
                timer.Elapsed += OnTick;
                timer.Start();
            }
        }

        private void OnTick(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Wait until previous tick finishes
            if (_handleFailingMessages) return;

            try
            {
                _handleFailingMessages = true;
                var messages = Directory.GetFiles(Options.FailedRequestPath, "*.json");
                var jsonSerializerSettings = GetJsonSerializerSettings();
                foreach (var messagePath in messages)
                {
                    var messageJson = File.ReadAllText(messagePath);
                    var message = JsonConvert.DeserializeObject<Message>(messageJson, jsonSerializerSettings);
                    try
                    {
                        Log(message);
                    }
                    finally
                    {
                        // We can delete this file weather or not it fails again. Failing requests will write a new json file to disk.
                        File.Delete(messagePath);
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _handleFailingMessages = false;
            }
        }

        /// <summary>
        /// Creates a instance of the logger. Do exactly the same as calling the constructor with a Guid parameter.
        /// </summary>
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
                message.Data = exception.ToDataList();
            }

            Log(message);
        }

        public string Log(Message message)
        {
            return EndLog(BeginLog(message, null, null));
        }

        public IAsyncResult BeginLog(Message message, AsyncCallback asyncCallback, object asyncState)
        {
            if (message.DateTime == DateTime.MinValue) message.DateTime = DateTime.UtcNow;
            if (OnMessage != null) OnMessage(this, new MessageEventArgs(message));

            var headers = new WebHeaderCollection { { HttpRequestHeader.ContentType, "application/json" } };

            var jsonSerializerSettings = GetJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);

            return Options.WebClient.Post(headers, ApiUrl(), json)
                             .ContinueWith(t =>
                             {
                                 if (t.Status != TaskStatus.RanToCompletion)
                                 {
                                     HandleError(message, t, json);
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

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            return jsonSerializerSettings;
        }

        private void HandleError(Message message, Task<string> t, string json)
        {
            if (OnMessageFail != null) OnMessageFail(this, new FailEventArgs(message, t.Exception));
            if (!string.IsNullOrWhiteSpace(Options.FailedRequestPath))
            {
                var filename = string.Format("{0}-{1}.json", message.DateTime.Ticks, DateTime.UtcNow.Ticks);
                File.WriteAllText(Path.Combine(Options.FailedRequestPath, filename), json);
            }
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
            return Options.WebClient.Get(ApiUrl(new NameValueCollection { { "id", id } }))
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

            return Options.WebClient.Get(url).ContinueWith(t =>
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
            return new Uri(Options.Url, "api/v2/messages" + q.ToQueryString());
        }
    }
}
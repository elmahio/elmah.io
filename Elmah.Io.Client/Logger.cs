using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mannex.Threading.Tasks;
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
        private readonly string _apiKey;
        private readonly Uri _url = new Uri("https://api.elmah.io/");
        private readonly TimeSpan _timeout = new TimeSpan(0, 0, 5);
        private readonly HttpClient _httpClient;

        public Guid LogId { get { return _logId; } }
        public Uri Url { get { return _url; } }

        public event EventHandler<MessageEventArgs> OnMessage;

        public event EventHandler<FailEventArgs> OnMessageFail;

        /// <summary>
        /// Creates a new logger with the specified log ID. The logger is configured to use elmah.io's API.
        /// This is probably the constructor you want to use 99 % of the times.
        /// </summary>
        public Logger(Guid logId, string apiKey) : this(logId, apiKey, null)
        {
        }

        ///// <summary>
        ///// Creates a new logger with the specified log ID and URL. This constructor is primarily ment for test
        ///// purposes or if you are using a version of the elmah.io API not at its official location.
        ///// </summary>
        public Logger(Guid logId, string apiKey, Uri url)
        {
            _logId = logId;
            _apiKey = apiKey;
            if (url != null) _url = url;
            _httpClient = new HttpClient {BaseAddress = _url, Timeout = _timeout};
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

        public Uri Log(Message message)
        {
            var logTask = LogAsync(message, null, null);
            logTask.Wait();
            return logTask.Result;
        }

        public Task<Uri> LogAsync(Message message, AsyncCallback asyncCallback, object asyncState)
        {
            if (message.DateTime == DateTime.MinValue) message.DateTime = DateTime.UtcNow;
            if (OnMessage != null) OnMessage(this, new MessageEventArgs(message));

            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);

            return _httpClient
                .PostAsync(string.Format("v3/messages/{0}?api_key={1}", _logId, _apiKey), new StringContent(json, Encoding.UTF8, "application/json"))
                .ContinueWith(t =>
                {
                    if (t.Status != TaskStatus.RanToCompletion || !t.Result.IsSuccessStatusCode)
                    {
                        if (OnMessageFail != null) OnMessageFail(this, new FailEventArgs(message, t.Result.ReasonPhrase, t.Exception));
                        return null;
                    }

                    return t.Result.Headers.Location;
                })
                .Apmize(asyncCallback, asyncState);
        }

        public Message GetMessage(string id)
        {
            var getMessageTask = GetMessageAsync(id, null, null);
            getMessageTask.Wait();
            return getMessageTask.Result;
        }

        public Task<Message> GetMessageAsync(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return _httpClient
                .GetAsync(string.Format("v3/messages/{0}/{1}?api_key={2}", _logId, id, _apiKey))
                .ContinueWith(t =>
                {
                    if (t.Status != TaskStatus.RanToCompletion || !t.Result.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var message = JsonConvert.DeserializeObject<Message>(t.Result.Content.ReadAsStringAsync().Result);
                    return message;
                })
                .Apmize(asyncCallback, asyncState);
        }

        public MessagesResult GetMessages(int pageIndex, int pageSize)
        {
            var getMessagesTask = GetMessagesAsync(pageIndex, pageSize, null, null);
            getMessagesTask.Wait();
            return getMessagesTask.Result;
        }

        public Task<MessagesResult> GetMessagesAsync(int pageIndex, int pageSize, AsyncCallback asyncCallback, object asyncState)
        {
            return _httpClient
                .GetAsync(string.Format("v3/messages/{0}?pageindex={1}&pagesize={2}&api_key={3}", _logId, pageIndex, pageSize, _apiKey))
                .ContinueWith(t =>
                {
                    if (t.Status != TaskStatus.RanToCompletion || !t.Result.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var messages = JsonConvert.DeserializeObject<MessagesResult>(t.Result.Content.ReadAsStringAsync().Result);
                    return messages;
                })
                .Apmize(asyncCallback, asyncState);
        }
    }
}
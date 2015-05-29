using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elmah.Io.Client;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog, IErrorLog
    {
        public static ILogger Client;

        /// <summary>
        /// ELMAH doesn't use this constructor and it is only published in order for you to create
        /// a new error logger using a custom implementation of ILogger or an instance of Logger that
        /// you've already created. If you implement your own ILogger, please  identify yourself
        /// using an appropriate user agent.
        /// </summary>
        public ErrorLog(ILogger logger)
        {
            Client = logger;
        }

        /// <summary>
        /// The constructor used by ELMAH. ELMAH provides key/value pairs in the config dictionary,
        /// as specified in attributes in the ELMAH XML configuration (web.config).
        /// This constructor intentionally handles the internal ILogger instance as singleton.
        /// ELMAH calls this constructor one time per error logged and to only create the logger
        /// once, letting you listen for events on the logger etc., the logger needs to be
        /// singleton.
        /// </summary>
        public ErrorLog(IDictionary config)
        {
            if (Client != null) return;
            
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (!config.Contains("LogId") && !config.Contains("LogIdKey"))
            {
                throw new ApplicationException(
                    "Missing LogId or LogIdKey. Please specify a LogId in your web.config like this: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogId=\"98895825-2516-43DE-B514-FFB39EA89A65\" /> or using AppSettings: <errorLog type=\"Elmah.Io.ErrorLog, Elmah.Io\" LogIdKey=\"MyAppSettingsKey\" /> where MyAppSettingsKey points to a AppSettings with the key 'MyAPpSettingsKey' and value equal LogId.");
            }

            var logId = config.LogId();
            var url = config.Url();
            var durable = config.Durable();
            var failedRequestPath = config.FailedRequestPath();
            ApplicationName = config.ApplicationName();

            Client =
                new LoggerConfiguration()
                    .UseLog(logId)
                    .WithOptions(new LoggerOptions {Durable = durable, Url = url, FailedRequestPath = failedRequestPath})
                    .CreateLogger();
        }

        public override string Log(Error error)
        {
            return EndLog(BeginLog(error, null, null));
        }

        public override IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState)
        {
            var message = new Message(error.Message)
            {
                Application = error.ApplicationName,
                Cookies = error.Cookies.AllKeys.Select(key => new Item {Key = key, Value = error.Cookies[key]}).ToList(),
                DateTime = error.Time,
                Detail = error.Detail,
                Form = error.Form.AllKeys.Select(key => new Item { Key = key, Value = error.Form[key] }).ToList(),
                Hostname = error.HostName,
                QueryString = error.QueryString.AllKeys.Select(key => new Item { Key = key, Value = error.QueryString[key] }).ToList(),
                ServerVariables = error.ServerVariables.AllKeys.Select(key => new Item { Key = key, Value = error.ServerVariables[key] }).ToList(),
                Title = error.Message,
                Source = error.Source,
                StatusCode = error.StatusCode,
                Type = error.Type,
                User = error.User,
                Data = error.Exception.ToDataList(),
            };

            return Client.BeginLog(message, asyncCallback, asyncState);
        }

        public override string EndLog(IAsyncResult asyncResult)
        {
            return EndImpl<string>(asyncResult);
        }

        public override IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return Client.BeginGetMessage(id, asyncCallback, asyncCallback);
        }

        public override ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            var message = EndImpl<Message>(asyncResult);
            var errorLogEntry = MapErrorLogEntry(message);
            return errorLogEntry;
        }

        private ErrorLogEntry MapErrorLogEntry(Message message)
        {
            var error = new Error
            {
                ApplicationName = message.Application,
                Detail = message.Detail,
                HostName = message.Hostname,
                Message = message.Title,
                Source = message.Source,
                StatusCode = message.StatusCode ?? 0,
                Time = message.DateTime,
                Type = message.Type,
                User = message.User,
            };

            foreach (var cookie in (message.Cookies ?? new List<Item>()))
            {
                error.Cookies.Add(cookie.Key, cookie.Value);
            }

            foreach (var formElement in (message.Form ?? new List<Item>()))
            {
                error.Form.Add(formElement.Key, formElement.Value);
            }

            foreach (var queryStringItem in (message.QueryString ?? new List<Item>()))
            {
                error.QueryString.Add(queryStringItem.Key, queryStringItem.Value);
            }

            foreach (var serverVariable in (message.ServerVariables ?? new List<Item>()))
            {
                error.ServerVariables.Add(serverVariable.Key, serverVariable.Value);
            }

            var errorLogEntry = new ErrorLogEntry(this, message.Id, error);
            return errorLogEntry;
        }

        public override ErrorLogEntry GetError(string id)
        {
            return EndGetError(BeginGetError(id, null, null));
        }

        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            return Client.BeginGetMessages(pageIndex, pageSize, asyncCallback, errorEntryList);
        }

        public override int EndGetErrors(IAsyncResult asyncResult)
        {
            var messagesResult = EndImpl<MessagesResult>(asyncResult);
            var errorEntryList = (IList)asyncResult.AsyncState;

            var entries =
                from Message m in (IEnumerable)messagesResult.Messages
                select MapErrorLogEntry(m);

            foreach (var entry in entries)
            {
                errorEntryList.Add(entry);
            }

            return messagesResult.Total;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            return EndGetErrors(BeginGetErrors(pageIndex, pageSize, errorEntryList, null, null));
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
    }
}
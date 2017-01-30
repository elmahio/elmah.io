using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;

namespace Elmah.Io
{
    public class ErrorLog : Elmah.ErrorLog, IErrorLog
    {
        public static IElmahioAPI Api;

        private readonly Guid _logId;

        public static IMessages Client => Api.Messages;

        /// <summary>
        /// ELMAH doesn't use this constructor and it is only published in order for you to create
        /// a new error logger using a custom implementation of ILogger or an instance of Logger that
        /// you've already created. If you implement your own ILogger, please  identify yourself
        /// using an appropriate user agent.
        /// </summary>
        public ErrorLog(IElmahioAPI logger)
        {
            Api = logger;
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
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _logId = config.LogId();
            var apiKey = config.ApiKey();
            ApplicationName = config.ApplicationName();

            if (Api != null) return;

            var url = config.Url();
            var elmahioApi = ElmahioAPI.Create(apiKey);
            if (url != null)
            {
                elmahioApi.BaseUri = url;
            }

            Api = elmahioApi;
        }

        public override string Log(Error error)
        {
            return EndLog(BeginLog(error, null, null));
        }

        public override IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState)
        {
            var tcs = new TaskCompletionSource<Message>(asyncState);
            Client
                .CreateAndNotifyAsync(_logId, new CreateMessage
                {
                    Application = error.ApplicationName,
                    Cookies = Itemize(error.Cookies),
                    DateTime = error.Time,
                    Detail = error.Detail,
                    Form = Itemize(error.Form),
                    Hostname = error.HostName,
                    QueryString = Itemize(error.QueryString),
                    ServerVariables = Itemize(error.ServerVariables),
                    Title = error.Message,
                    Source = error.Source,
                    StatusCode = error.StatusCode,
                    Type = error.Type,
                    User = error.User,
                    Data = error.Exception.ToDataList(),
                })
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        public override string EndLog(IAsyncResult asyncResult)
        {
            var message = EndImpl<Message>(asyncResult);
            return message?.Id;
        }

        public override IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            var tcs = new TaskCompletionSource<Message>(asyncState);
            Client
                .GetAsync(id, _logId.ToString())
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        public override ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            var message = EndImpl<Message>(asyncResult);
            if (message == null) return null;
            var errorLogEntry = MapErrorLogEntry(message);
            return errorLogEntry;
        }

        public override ErrorLogEntry GetError(string id)
        {
            return EndGetError(BeginGetError(id, null, null));
        }

        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            var tcs = new TaskCompletionSource<MessagesResult>(errorEntryList);
            Client
                .GetAllAsync(_logId.ToString(), pageIndex, pageSize)
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        public override int EndGetErrors(IAsyncResult asyncResult)
        {
            var messagesResult = EndImpl<MessagesResult>(asyncResult);
            if (messagesResult == null) return 0;
            var errorEntryList = (IList)asyncResult.AsyncState;

            var entries = messagesResult.Messages.Select(MapErrorOverviewEntry);

            foreach (var entry in entries)
            {
                errorEntryList.Add(entry);
            }

            return messagesResult.Total ?? 0;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            return EndGetErrors(BeginGetErrors(pageIndex, pageSize, errorEntryList, null, null));
        }

        private T EndImpl<T>(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException(nameof(asyncResult));
            }

            var task = asyncResult as Task<T>;
            if (task == null)
            {
                throw new ArgumentException(null, nameof(asyncResult));
            }

            return task.Result;
        }

        private static void Continue<T>(AsyncCallback asyncCallback, Task<T> t, TaskCompletionSource<T> tcs)
        {
            // Copy the task result into the returned task.
            if (t.IsFaulted && t.Exception != null)
            {
                tcs.TrySetException(t.Exception.InnerExceptions);
            }
            else if (t.IsCanceled)
            {
                tcs.TrySetCanceled();
            }
            else
            {
                tcs.TrySetResult(t.Result);
            }

            asyncCallback?.Invoke(tcs.Task);
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
                Time = message.DateTime.Value,
                Type = message.Type,
                User = message.User,
            };

            (message.Cookies ?? new List<Item>()).ToList().ForEach(c => error.Cookies.Add(c.Key, c.Value));
            (message.Form ?? new List<Item>()).ToList().ForEach(c => error.Form.Add(c.Key, c.Value));
            (message.QueryString ?? new List<Item>()).ToList().ForEach(c => error.QueryString.Add(c.Key, c.Value));
            (message.ServerVariables ?? new List<Item>()).ToList().ForEach(c => error.ServerVariables.Add(c.Key, c.Value));

            var errorLogEntry = new ErrorLogEntry(this, message.Id, error);
            return errorLogEntry;
        }

        private ErrorLogEntry MapErrorOverviewEntry(MessageOverview message)
        {
            var error = new Error
            {
                ApplicationName = message.Application,
                Detail = message.Detail,
                HostName = message.Hostname,
                Message = message.Title,
                Source = message.Source,
                StatusCode = message.StatusCode ?? 0,
                Time = message.DateTime.Value,
                Type = message.Type,
                User = message.User,
            };

            var errorLogEntry = new ErrorLogEntry(this, message.Id, error);
            return errorLogEntry;
        }

        private IList<Item> Itemize(NameValueCollection nameValues)
        {
            return nameValues.AllKeys.Select(key => new Item { Key = key, Value = nameValues[key] }).ToList();
        }
    }
}
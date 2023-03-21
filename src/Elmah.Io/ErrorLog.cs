using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Elmah.Io.Client;

namespace Elmah.Io
{
    /// <summary>
    /// <see cref="ErrorLog"/>
    /// </summary>
    public class ErrorLog : global::Elmah.ErrorLog, IErrorLog
    {
        internal static string _assemblyVersion = typeof(ErrorLog).Assembly.GetName().Version.ToString();
        internal static string _systemWebAssemblyVersion = typeof(HttpApplication).Assembly.GetName().Version.ToString();

        /// <summary>
        /// The IElmahioAPI from the Elmah.Io.Client package that is used internally in this error log.
        /// </summary>
        public static IElmahioAPI Api;

        private readonly Guid _logId;

        /// <summary>
        /// The IMessagesClient from the Elmah.Io.Client package that is used internally in this error log.
        /// </summary>
        public static IMessagesClient Client => Api.Messages;

        /// <summary>
        /// ELMAH doesn't use this constructor and it is only published in order for you to create
        /// a new error logger using a custom implementation of ILogger or an instance of Logger that
        /// you've already created. If you implement your own ILogger, please  identify yourself
        /// using an appropriate user agent.
        /// </summary>
        public ErrorLog(IElmahioAPI logger, Guid logId)
        {
            Api = logger;
            _logId = logId;
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

            var elmahioApi = ElmahioAPI.Create(apiKey, new ElmahIoOptions
            {
                Timeout = new TimeSpan(0, 0, 5),
                UserAgent = UserAgent(),
            });

            Api = elmahioApi;
        }

        /// <summary>
        /// <see cref="IErrorLog.Log(Error)"/>
        /// </summary>
        public override string Log(Error error)
        {
            return EndLog(BeginLog(error, null, null));
        }

        /// <summary>
        /// <see cref="IErrorLog.BeginLog(Error, AsyncCallback, object)"/>
        /// </summary>
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
                    StatusCode = StatusCode(error),
                    Type = error.Type,
                    User = error.User,
                    Data = Data(error.Exception),
                }, default)
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        /// <summary>
        /// <see cref="IErrorLog.EndLog(IAsyncResult)"/>
        /// </summary>
        public override string EndLog(IAsyncResult asyncResult)
        {
            var message = EndImpl<Message>(asyncResult);
            return message?.Id;
        }

        /// <summary>
        /// <see cref="IErrorLog.BeginGetError(string, AsyncCallback, object)"/>
        /// </summary>
        public override IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState)
        {
            var tcs = new TaskCompletionSource<Message>(asyncState);
            Client
                .GetAsync(id, _logId.ToString())
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        /// <summary>
        /// <see cref="IErrorLog.EndGetError(IAsyncResult)"/>
        /// </summary>
        public override ErrorLogEntry EndGetError(IAsyncResult asyncResult)
        {
            var message = EndImpl<Message>(asyncResult);
            if (message == null) return null;
            var errorLogEntry = MapErrorLogEntry(message);
            return errorLogEntry;
        }

        /// <summary>
        /// <see cref="IErrorLog.GetError(string)"/>
        /// </summary>
        public override ErrorLogEntry GetError(string id)
        {
            return EndGetError(BeginGetError(id, null, null));
        }

        /// <summary>
        /// <see cref="IErrorLog.BeginGetErrors(int, int, IList, AsyncCallback, object)"/>
        /// </summary>
        public override IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            var tcs = new TaskCompletionSource<MessagesResult>(errorEntryList);
            Client
                .GetAllAsync(_logId.ToString(), pageIndex, pageSize)
                .ContinueWith(t => Continue(asyncCallback, t, tcs));
            return tcs.Task;
        }

        /// <summary>
        /// <see cref="IErrorLog.EndGetErrors(IAsyncResult)"/>
        /// </summary>
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

        /// <summary>
        /// <see cref="IErrorLog.GetErrors(int, int, IList)"/>
        /// </summary>
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
                Time = message.DateTime.Value.DateTime,
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
                Time = message.DateTime.Value.DateTime,
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

        private static int? StatusCode(Error error)
        {
            if (error.Exception != null)
            {
                // If an exception is thrown, but the response has a successful status code,
                // it is because the correct status code is not yet assigned.
                // Override it with 500.
                return error.StatusCode < 400 ? 500 : error.StatusCode;
            }

            return error.StatusCode;
        }

        private IList<Item> Data(Exception exception)
        {
            if (exception == null) return null;
            var items = new List<Item>();
            var dataItems = exception.ToDataList();
            if (dataItems.Count > 0)
            {
                items.AddRange(dataItems);
            }

            if (exception is ExternalException ee)
            {
                items.Add(new Item { Key = ee.ItemName(nameof(ee.ErrorCode)), Value = ee.ErrorCode.ToString() });
            }

            if (exception is HttpException he)
            {
                items.Add(new Item { Key = he.ItemName(nameof(he.WebEventCode)), Value = he.WebEventCode.ToString() });
            }

            if (exception is HttpParseException hpe)
            {
                items.Add(new Item { Key = hpe.ItemName(nameof(hpe.FileName)), Value = hpe.FileName ?? string.Empty });
                items.Add(new Item { Key = hpe.ItemName(nameof(hpe.Line)), Value = hpe.Line.ToString() });
                items.Add(new Item { Key = hpe.ItemName(nameof(hpe.VirtualPath)), Value = hpe.VirtualPath ?? string.Empty });
            }

            return items;
        }

        private string UserAgent()
        {
            return new StringBuilder()
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io", _assemblyVersion)).ToString())
                .Append(" ")
                .Append(new ProductInfoHeaderValue(new ProductHeaderValue("System.Web", _systemWebAssemblyVersion)).ToString())
                .ToString();
        }
    }
}
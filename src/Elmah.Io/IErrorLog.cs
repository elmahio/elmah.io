using System;
using System.Collections;

namespace Elmah.Io
{
    /// <summary>
    /// ELMAH ErrorLog implementation that logs error to elmah.io.
    /// </summary>
    public interface IErrorLog
    {
        /// <summary>
        /// Log an error to elmah.io.
        /// </summary>
        /// <returns>The assigned elmah.io ID for the created error.</returns>
        string Log(Error error);
        /// <summary>
        /// Asynchronous version of the Log method.
        /// </summary>
        IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState);
        /// <summary>
        /// Asynchronous version of the Log method.
        /// </summary>
        string EndLog(IAsyncResult asyncResult);

        /// <summary>
        /// Return an error in elmah.io from its ID.
        /// </summary>
        ErrorLogEntry GetError(string id);
        /// <summary>
        /// Asynchronous version of the GetError method.
        /// </summary>
        IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState);
        /// <summary>
        /// Asynchronous version of the GetError method.
        /// </summary>
        ErrorLogEntry EndGetError(IAsyncResult asyncResult);

        /// <summary>
        /// Get a list of errors in elmah.io.
        /// </summary>
        int GetErrors(int pageIndex, int pageSize, IList errorEntryList);
        /// <summary>
        /// Asynchronous version of the GetErrors method.
        /// </summary>
        IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState);
        /// <summary>
        /// Asynchronous version of the GetErrors method.
        /// </summary>
        int EndGetErrors(IAsyncResult asyncResult);
    }
}
﻿using System;
using System.Collections;
using Elmah.Io.Client;

namespace Elmah.Io
{
    public interface IErrorLog
    {
        ILogger Logger { get; }

        string Log(Error error);
        IAsyncResult BeginLog(Error error, AsyncCallback asyncCallback, object asyncState);
        string EndLog(IAsyncResult asyncResult);

        ErrorLogEntry GetError(string id);
        IAsyncResult BeginGetError(string id, AsyncCallback asyncCallback, object asyncState);
        ErrorLogEntry EndGetError(IAsyncResult asyncResult);

        int GetErrors(int pageIndex, int pageSize, IList errorEntryList);
        IAsyncResult BeginGetErrors(int pageIndex, int pageSize, IList errorEntryList, AsyncCallback asyncCallback, object asyncState);
        int EndGetErrors(IAsyncResult asyncResult);
    }
}
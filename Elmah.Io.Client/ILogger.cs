using System;

namespace Elmah.Io.Client
{
    public interface ILogger
    {
        string Log(Message message);
        IAsyncResult BeginLog(Message message, AsyncCallback asyncCallback, object asyncState);
        string EndLog(IAsyncResult asyncResult);

        Message GetMessage(string id);
        IAsyncResult BeginGetMessage(string id, AsyncCallback asyncCallback, object asyncState);
        Message EndGetMessage(IAsyncResult asyncResult);

        MessagesResult GetMessages(int pageIndex, int pageSize);
        IAsyncResult BeginGetMessages(int pageIndex, int pageSize, AsyncCallback asyncCallback, object asyncState);
        MessagesResult EndGetMessages(IAsyncResult asyncResult);

        void Verbose(string messageTemplate, params object[] propertyValues);
        void Verbose(Exception exception, string messageTemplate, params object[] propertyValues);
        void Debug(string messageTemplate, params object[] propertyValues);
        void Debug(Exception exception, string messageTemplate, params object[] propertyValues);
        void Information(string messageTemplate, params object[] propertyValues);
        void Information(Exception exception, string messageTemplate, params object[] propertyValues);
        void Warning(string messageTemplate, params object[] propertyValues);
        void Warning(Exception exception, string messageTemplate, params object[] propertyValues);
        void Error(string messageTemplate, params object[] propertyValues);
        void Error(Exception exception, string messageTemplate, params object[] propertyValues);
        void Fatal(string messageTemplate, params object[] propertyValues);
        void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
    }
}
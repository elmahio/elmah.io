using System;

namespace Elmah.Io.Client
{
    public class FailEventArgs : EventArgs
    {
        public FailEventArgs(Message message, string reason, Exception exception)
        {
            Message = message;
            Reason = reason;
            Exception = exception;
        }

        public Message Message { get; set; }
        public string Reason { get; set; }
        public Exception Exception { get; set; }
    }
}
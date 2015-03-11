using System;

namespace Elmah.Io.Client
{
    public class FailEventArgs : EventArgs
    {
        public FailEventArgs(Message message, Exception error)
        {
            Message = message;
            Error = error;
        }

        public Message Message { get; set; }

        public Exception Error { get; set; }
    }
}
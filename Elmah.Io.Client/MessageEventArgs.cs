using System;

namespace Elmah.Io.Client
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(Message message)
        {
            Message = message;
        }

        public Message Message { get; set; }
    }
}
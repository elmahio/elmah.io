using System;

namespace Elmah.Io.Client
{
    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
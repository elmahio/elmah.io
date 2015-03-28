using System.Collections.Generic;

namespace Elmah.Io.Client
{
    public class MessagesResult
    {
        public int Total { get; set; }

        public List<Message> Messages { get; set; }
    }
}
using System;

namespace Fint.Sse
{
    public class ServerSentEventReceivedEventArgs : EventArgs
    {
        public ServerSentEvent Message { get; private set; }
        public ServerSentEventReceivedEventArgs(ServerSentEvent message)
        {
            Message = message;
        }

    }
}

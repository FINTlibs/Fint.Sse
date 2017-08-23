using System;

namespace Fint.Sse
{
    public class StateChangedEventArgs : EventArgs
    {
        public EventSourceState State { get; private set; }
        public StateChangedEventArgs(EventSourceState state)
        {
            State = state;
        }
    }
}

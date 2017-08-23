using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fint.Sse
{
    interface IConnectionState
    {
        EventSourceState State { get; }
        Task<IConnectionState> Run(Action<ServerSentEvent> MsgReceivedCallback, CancellationToken cancelToken);
    }
}

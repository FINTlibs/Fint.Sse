using Fint.Event.Model;

namespace Fint.Sse
{
    public interface IEventHandler
    {
        void HandleEvent(SseEndpoint endpoint, Event<object> serverSideEvent);
    }
}
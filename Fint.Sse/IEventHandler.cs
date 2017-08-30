using Fint.Event.Model;

namespace Fint.Sse
{
    public interface IEventHandler
    {
        void HandleEvent(Event<object> serverSideEvent);
    }
}
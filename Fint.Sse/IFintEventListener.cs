namespace Fint.Sse
{
    public interface IFintEventListener
    {
        void Listen(string orgId);
        void Disconnect();
    }
}
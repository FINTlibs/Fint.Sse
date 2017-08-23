using System.Threading;

namespace Fint.Sse.Tests
{
    static class TestExtensions
    {
        public static void WaitOrThrow(this ManualResetEvent mre)
        {
#if DEBUG
            mre.WaitOne();
#else
            if (!mre.WaitOne(1000))
                throw new TimeoutException("Timeout waiting for manualresetevent");
#endif
        }
    }
}

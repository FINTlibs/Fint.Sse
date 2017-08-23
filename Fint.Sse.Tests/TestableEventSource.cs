using System;
using System.Collections.Generic;

namespace Fint.Sse.Tests
{
    class TestableEventSource : EventSource
    {
        public TestableEventSource(Uri url,IWebRequesterFactory factory) : base(url,factory)
        {

        }
        public TestableEventSource(Uri url, IWebRequesterFactory factory, Dictionary<string, string> headers) : base(url, factory, headers)
        {

        }
    }
}

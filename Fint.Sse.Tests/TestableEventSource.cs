using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Fint.Sse.Tests
{
    class TestableEventSource : EventSource
    {
        public TestableEventSource(Uri url,IWebRequesterFactory factory, ILogger logger) : base(url,factory,logger)
        {

        }
        public TestableEventSource(Uri url, IWebRequesterFactory factory, Dictionary<string, string> headers, ILogger logger) : base(url,factory,headers,logger)
        {

        }
    }
}

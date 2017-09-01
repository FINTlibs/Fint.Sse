using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Fint.Sse.Tests
{
    public class AbstractEventSourceTest
    {
        //[Fact]
        //public void TestSuccesfulConnectionWithHeaders()
        //{
        //    // setup
        //    Uri url = new Uri("http://test.com");
        //    CancellationTokenSource cts = new CancellationTokenSource();
        //    List<EventSourceState> states = new List<EventSourceState>();
        //    ServiceResponseMock response = new ServiceResponseMock(url, System.Net.HttpStatusCode.OK);
        //    WebRequesterFactoryMock factory = new WebRequesterFactoryMock(response);
        //    ManualResetEvent stateIsOpen = new ManualResetEvent(false);

        //    var headers = new Dictionary<string, string>
        //    {
        //        { "x-key", "headerValue" }
        //    };

        //    var es = new TestableAbstractEventSource(url, factory, headers);
        //    es.StateChanged += (o, e) =>
        //    {
        //        states.Add(e.State);
        //        if (e.State == EventSourceState.OPEN)
        //        {
        //            stateIsOpen.Set();
        //            cts.Cancel();
        //        }
        //    };


        //    // act
        //    stateIsOpen.Reset();

        //    es.Start(cts.Token);

        //    stateIsOpen.WaitOne();

        //    // assert
        //    Assert.Equal(1, factory.WebRequesterMock.Response.Headers.Count);
        //    Assert.Equal("headerValue", factory.WebRequesterMock.Response.Headers["x-key"]);
        //}

        [Fact]
        public void TestCreatesSecondConnectionAfterSpecifiedWaitTime()
        {
            
        }
    }
}
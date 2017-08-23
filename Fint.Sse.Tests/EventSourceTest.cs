using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Fint.Sse.Tests
{
    public class EventSourceTest
    {
        [Fact]
        public void TestFailedConnection()
        {
            // setup
            Uri url = new Uri("http://test.com");
            CancellationTokenSource cts = new CancellationTokenSource();
            List<EventSourceState> states = new List<EventSourceState>();
            ServiceResponseMock response = new ServiceResponseMock(url, System.Net.HttpStatusCode.NotFound);
            WebRequesterFactoryMock factory = new WebRequesterFactoryMock(response);
            ManualResetEvent stateIsClosed = new ManualResetEvent(false);

            TestableEventSource es = new TestableEventSource(url, factory);
            es.StateChanged += (o, e) =>
            {
                states.Add(e.State);
                if (e.State == EventSourceState.CLOSED)
                {
                    stateIsClosed.Set();
                    cts.Cancel();
                }
            };


            // act
            stateIsClosed.Reset();

            es.Start(cts.Token);

            stateIsClosed.WaitOne();

            // assert
            Assert.True(states.Count == 2);
            Assert.Equal(states[0], EventSourceState.CONNECTING);
            Assert.Equal(states[1], EventSourceState.CLOSED);
        }

        [Fact]
        public void TestSuccesfulConnection()
        {
            // setup
            Uri url = new Uri("http://test.com");
            CancellationTokenSource cts = new CancellationTokenSource();
            List<EventSourceState> states = new List<EventSourceState>();
            ServiceResponseMock response = new ServiceResponseMock(url, System.Net.HttpStatusCode.OK);
            WebRequesterFactoryMock factory = new WebRequesterFactoryMock(response);
            ManualResetEvent stateIsOpen = new ManualResetEvent(false);

            TestableEventSource es = new TestableEventSource(url, factory);
            es.StateChanged += (o, e) =>
            {
                states.Add(e.State);
                if (e.State == EventSourceState.OPEN)
                {
                    stateIsOpen.Set();
                    cts.Cancel();
                }
            };


            // act
            stateIsOpen.Reset();

            es.Start(cts.Token);

            stateIsOpen.WaitOne();

            // assert
            Assert.True(states.Count == 2);
            Assert.Equal(states[0], EventSourceState.CONNECTING);
            Assert.Equal(states[1], EventSourceState.OPEN);
        }


        [Fact]
        public void TestSuccesfulConnectionWithHeaders()
        {
            // setup
            Uri url = new Uri("http://test.com");
            CancellationTokenSource cts = new CancellationTokenSource();
            List<EventSourceState> states = new List<EventSourceState>();
            ServiceResponseMock response = new ServiceResponseMock(url, System.Net.HttpStatusCode.OK);
            WebRequesterFactoryMock factory = new WebRequesterFactoryMock(response);
            ManualResetEvent stateIsOpen = new ManualResetEvent(false);

            var headers = new Dictionary<string, string>
            {
                { "x-key", "headerValue" }
            };

            TestableEventSource es = new TestableEventSource(url, factory, headers);
            es.StateChanged += (o, e) =>
            {
                states.Add(e.State);
                if (e.State == EventSourceState.OPEN)
                {
                    stateIsOpen.Set();
                    cts.Cancel();
                }
            };


            // act
            stateIsOpen.Reset();

            es.Start(cts.Token);

            stateIsOpen.WaitOne();

            // assert
            Assert.Equal(1, factory.WebRequesterMock.Response.Headers.Count);
            Assert.Equal("headerValue", factory.WebRequesterMock.Response.Headers["x-key"]);
        }
    }
}

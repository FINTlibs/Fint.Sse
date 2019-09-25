using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Fint.Sse
{
    public class EventSource
    {
        //TODO: private static readonly ILogger _logger = new LoggerFactory().CreateLogger<EventSource>();

        public event EventHandler<StateChangedEventArgs> StateChanged;
        public event EventHandler<ServerSentEventReceivedEventArgs> EventReceived;

        public CancellationTokenSource CancellationToken { get; set; }

        private IWebRequesterFactory _webRequesterFactory = new WebRequesterFactory();
        private int _timeout = 0;
        public Uri Url { get; private set; }
        public EventSourceState State { get { return CurrentState.State; } }
        public string LastEventId { get; private set; }
        private IConnectionState _currentState = null;
        private CancellationToken _stopToken;
        private CancellationTokenSource mTokenSource = new CancellationTokenSource();
        private Dictionary<string, string> _headers;
        public ILogger _logger;

        private IConnectionState CurrentState
        {
            get { return _currentState; }
            set
            {
                if (!value.Equals(_currentState))
                {
                    _logger.LogDebug("State changed from {mCurrentState} to {value}", _currentState, value);
                    _currentState = value;
                    OnStateChanged(_currentState.State);
                }
            }
        }

        public EventSource(Uri url, int timeout)
        {
            Initialize(url, timeout, null);
        }

        public EventSource(Uri url, Dictionary<string, string> headers, int timeout, ITokenService tokenService, ILogger logger)
        {
            _headers = headers;
            _logger = logger;
            Initialize(url, timeout, tokenService);
        }

        /// <summary>
        /// Constructor for testing purposes
        /// </summary>
        /// <param name="factory">The factory that generates the WebRequester to use.</param>
        protected EventSource(Uri url, IWebRequesterFactory factory, ILogger logger)
        {
            _webRequesterFactory = factory;
            _logger = logger;
            Initialize(url, 0, null);
        }

        protected EventSource(Uri url, IWebRequesterFactory factory, Dictionary<string, string> headers, ILogger logger)
        {
            _webRequesterFactory = factory;
            _headers = headers;
            _logger = logger;
            Initialize(url, 0, null);
        }

        private void Initialize(Uri url, int timeout, ITokenService tokenService)
        {
            _timeout = timeout;
            Url = url;
            CurrentState = new DisconnectedState(Url, _webRequesterFactory, _headers, tokenService, _logger);
            _logger.LogInformation("EventSource created for {url} \\o/", url);
        }


        /// <summary>
        /// Start the EventSource. 
        /// </summary>
        /// <param name="stopToken">Cancel this token to stop the EventSource.</param>
        public void Start(CancellationToken stopToken)
        {
            if (State == EventSourceState.CLOSED)
            {
                _stopToken = stopToken;
                mTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stopToken);
                Run();
            }
        }

        protected void Run()
        {
            if (mTokenSource.IsCancellationRequested && CurrentState.State == EventSourceState.CLOSED)
                return;

            _currentState.Run(this.OnEventReceived, mTokenSource.Token).ContinueWith(cs =>
            {
                CurrentState = cs.Result;
                Run();
            });
        }

        protected void OnEventReceived(ServerSentEvent sse)
        {
            if (EventReceived != null)
            {
                EventReceived(this, new ServerSentEventReceivedEventArgs(sse));
            }
        }

        protected void OnStateChanged(EventSourceState newState)
        {
            if (StateChanged != null)
            {
                StateChanged(this, new StateChangedEventArgs(newState));
            }
        }
    }
}

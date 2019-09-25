using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fint.Sse
{
    class DisconnectedState : IConnectionState 
    {
        private Uri _url;
        private IWebRequesterFactory _webRequesterFactory;
        private Dictionary<string, string> _headers;
        private ITokenService _tokenService;
        private ILogger _logger;

        public EventSourceState State
        {
            get { return EventSourceState.CLOSED; }
        }

        public DisconnectedState(Uri url, IWebRequesterFactory webRequesterFactory, Dictionary<string, string> headers, ITokenService tokenService, ILogger logger)
        {
            if (url == null) throw new ArgumentNullException("Url cant be null");
            _url = url;
            _webRequesterFactory = webRequesterFactory;
            _headers = headers;
            _tokenService = tokenService;
            _logger = logger;
        }

        public Task<IConnectionState> Run(Action<ServerSentEvent> donothing, CancellationToken cancelToken)
        {
            if(cancelToken.IsCancellationRequested)
                return Task.Factory.StartNew<IConnectionState>(() => { return new DisconnectedState(_url, _webRequesterFactory, _headers, _tokenService, _logger); });
            else
                return Task.Factory.StartNew<IConnectionState>(() => { return new ConnectingState(_url, _webRequesterFactory, _headers, _tokenService, _logger); });
        }
    }
}

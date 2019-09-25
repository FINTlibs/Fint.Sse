using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fint.Sse
{
    class ConnectingState : IConnectionState
    {
        private Uri _url;
        private IWebRequesterFactory _webRequesterFactory;
        private Dictionary<string, string> _headers;
        private ITokenService _tokenService;
        private ILogger _logger;

        public EventSourceState State { get { return EventSourceState.CONNECTING; } }
        
        public ConnectingState(Uri url, IWebRequesterFactory webRequesterFactory, Dictionary<string, string> headers, ITokenService tokenService, ILogger logger)
        {
            if (url == null) throw new ArgumentNullException("Url cant be null");
            if (webRequesterFactory == null) throw new ArgumentNullException("Factory cant be null");
            _url = url;
            _webRequesterFactory = webRequesterFactory;
            _headers = headers;
            _tokenService = tokenService;
            _logger = logger;
        }

        public Task<IConnectionState> Run(Action<ServerSentEvent> donothing, CancellationToken cancelToken)
        {
            IWebRequester requester = _webRequesterFactory.Create();
            var taskResp = requester.Get(_url, _tokenService, _headers);

            return taskResp.ContinueWith<IConnectionState>(tsk => 
            {
                if (tsk.Status == TaskStatus.RanToCompletion && !cancelToken.IsCancellationRequested)
                {
                    IServerResponse response = tsk.Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return new ConnectedState(response, _webRequesterFactory, _headers, _tokenService, _logger);
                    }
                    else
                    {
                        _logger.LogInformation("Failed to connect to: " + _url.ToString() + response ?? (" Http statuscode: " + response.StatusCode));
                    }
                }
                else
                {
                    _logger.LogDebug(tsk.Exception, "Task Status {@Status}", tsk.Status);
                }

                return new DisconnectedState(_url, _webRequesterFactory, _headers, _tokenService, _logger);
            });
        }
    }
}

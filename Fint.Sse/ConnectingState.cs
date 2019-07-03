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
        private Uri mUrl;
        private IWebRequesterFactory mWebRequesterFactory;
        private Dictionary<string, string> mHeaders;
        private ITokenService mTokenService;
        private ILogger mLogger;

        public EventSourceState State { get { return EventSourceState.CONNECTING; } }
        
        public ConnectingState(Uri url, IWebRequesterFactory webRequesterFactory, Dictionary<string, string> headers, ITokenService tokenService, ILogger logger)
        {
            if (url == null) throw new ArgumentNullException("Url cant be null");
            if (webRequesterFactory == null) throw new ArgumentNullException("Factory cant be null");
            mUrl = url;
            mWebRequesterFactory = webRequesterFactory;
            mHeaders = headers;
            mTokenService = tokenService;
            mLogger = logger;
        }

        public Task<IConnectionState> Run(Action<ServerSentEvent> donothing, CancellationToken cancelToken)
        {
            IWebRequester requester = mWebRequesterFactory.Create();
            var taskResp = requester.Get(mUrl, mTokenService, mHeaders);

            return taskResp.ContinueWith<IConnectionState>(tsk => 
            {
                if (tsk.Status == TaskStatus.RanToCompletion && !cancelToken.IsCancellationRequested)
                {
                    IServerResponse response = tsk.Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return new ConnectedState(response, mWebRequesterFactory, mHeaders, mTokenService, mLogger);
                    }
                    else
                    {
                        mLogger.LogInformation("Failed to connect to: " + mUrl.ToString() + response ?? (" Http statuscode: " + response.StatusCode));
                    }
                }
                else
                {
                    mLogger.LogDebug(tsk.Exception, "Task Status {@Status}", tsk.Status);
                }

                return new DisconnectedState(mUrl, mWebRequesterFactory, mHeaders, mTokenService, mLogger);
            });
        }
    }
}

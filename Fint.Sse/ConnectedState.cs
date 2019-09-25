using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fint.Sse
{
    class ConnectedState : IConnectionState
    {
        private IWebRequesterFactory _webRequesterFactory;
        private ServerSentEvent _sse = null;
        private string _remainingText = string.Empty;   // the text that is not ended with a lineending char is saved for next call.
        private IServerResponse _response;
        private Dictionary<string, string> _headers;
        private ITokenService _tokenService;
        private ILogger _logger;

        public EventSourceState State => EventSourceState.OPEN;

        public ConnectedState(IServerResponse response, IWebRequesterFactory webRequesterFactory, Dictionary<string, string> headers, ITokenService tokenService, ILogger logger)
        {
            _response = response;
            _webRequesterFactory = webRequesterFactory;
            _headers = headers;
            _tokenService = tokenService;
            _logger = logger;
        }

        public Task<IConnectionState> Run(Action<ServerSentEvent> msgReceived, CancellationToken cancelToken)
        {
            Task<IConnectionState> t = new Task<IConnectionState>(() =>
            {
                {
                    var stream = _response.GetResponseStream();
                    {
                        byte[] buffer = new byte[1024 * 8];
                        var taskRead = stream.ReadAsync(buffer, 0, buffer.Length, cancelToken);

                        try
                        {
                            taskRead.Wait(cancelToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "ConnectedState.Run");
                        }
                        if (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                var bytesRead = taskRead.Result;

                                if (bytesRead > 0) // stream has not reached the end yet
                                {
                                    _logger.LogTrace("ReadCallback {bytesRead} bytesRead", bytesRead);
                                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                    text = _remainingText + text;
                                    string[] lines = StringSplitter.SplitIntoLines(text, out _remainingText);
                                    foreach (string line in lines)
                                    {
                                        if (cancelToken.IsCancellationRequested) break;

                                        if (string.IsNullOrEmpty(line.Trim()) && _sse != null)
                                        {
                                            _logger.LogDebug("Message received");
                                            msgReceived(_sse);
                                            _sse = null;
                                        }
                                        else if (line.StartsWith(":"))
                                        {
                                            _logger.LogDebug("A comment was received: {line}", line);
                                        }
                                        else
                                        {
                                            string fieldName = String.Empty;
                                            string fieldValue = String.Empty;
                                            if (line.Contains(':'))
                                            {
                                                int index = line.IndexOf(':');
                                                fieldName = line.Substring(0, index);
                                                fieldValue = line.Substring(index + 1).TrimStart();
                                            }
                                            else
                                                fieldName = line;

                                            if (String.Compare(fieldName, "event", true) == 0)
                                            {
                                                _sse = _sse ?? new ServerSentEvent();
                                                _sse.EventType = fieldValue;
                                            }
                                            else if (String.Compare(fieldName, "data", true) == 0)
                                            {
                                                _sse = _sse ?? new ServerSentEvent();
                                                _sse.Data = fieldValue + '\n';
                                            }
                                            else if (String.Compare(fieldName, "id", true) == 0)
                                            {
                                                _sse = _sse ?? new ServerSentEvent();
                                                _sse.LastEventId = fieldValue;
                                            }
                                            else if (String.Compare(fieldName, "retry", true) == 0)
                                            {
                                                int parsedRetry;
                                                if (int.TryParse(fieldValue, out parsedRetry))
                                                {
                                                    _sse = _sse ?? new ServerSentEvent();
                                                    _sse.Retry = parsedRetry;
                                                }
                                            }
                                            else
                                            {
                                                _logger.LogInformation("An unknown line was received: {line}", line);
                                            }
                                        }
                                    }

                                    if (!cancelToken.IsCancellationRequested)
                                        return this;
                                }
                                else // end of the stream reached
                                {
                                    _logger.LogDebug("No bytes read. End of stream.");
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation(ex, "ConnectedState.Run");
                            }
                        }

                        //stream.Dispose()
                        //stream.Close();
                        //mResponse.Close();
                        //mResponse.Dispose();
                        return new DisconnectedState(_response.ResponseUri, _webRequesterFactory, _headers, _tokenService, _logger);
                    }
                }
            });

            t.Start();
            return t;
        }
    }
}

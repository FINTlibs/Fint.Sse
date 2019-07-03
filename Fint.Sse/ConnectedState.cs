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
        private IWebRequesterFactory mWebRequesterFactory;
        private ServerSentEvent mSse = null;
        private string mRemainingText = string.Empty;   // the text that is not ended with a lineending char is saved for next call.
        private IServerResponse mResponse;
        private Dictionary<string, string> mHeaders;
        private ITokenService mTokenService;
        private ILogger mLogger;

        public EventSourceState State { get { return EventSourceState.OPEN; } }

        public ConnectedState(IServerResponse response, IWebRequesterFactory webRequesterFactory, Dictionary<string, string> headers, ITokenService tokenService, ILogger logger)
        {
            mResponse = response;
            mWebRequesterFactory = webRequesterFactory;
            mHeaders = headers;
            mTokenService = tokenService;
            mLogger = logger;
        }

        public Task<IConnectionState> Run(Action<ServerSentEvent> msgReceived, CancellationToken cancelToken)
        {
            int i = 0;

            Task<IConnectionState> t = new Task<IConnectionState>(() =>
            {
                //using (mResponse)
                {
                    //using (var stream = mResponse.GetResponseStream())
                    var stream = mResponse.GetResponseStream();
                    {
                        byte[] buffer = new byte[1024 * 8];
                        var taskRead = stream.ReadAsync(buffer, 0, buffer.Length, cancelToken);

                        try
                        {
                            taskRead.Wait(cancelToken);
                        }
                        catch (Exception ex)
                        {
                            mLogger.LogWarning(ex, "ConnectedState.Run");
                        }
                        if (!cancelToken.IsCancellationRequested)
                        {
                            try
                            {
                                var bytesRead = taskRead.Result;

                                if (bytesRead > 0) // stream has not reached the end yet
                                {
                                    mLogger.LogTrace("ReadCallback {bytesRead} bytesRead", bytesRead);
                                    string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                    text = mRemainingText + text;
                                    string[] lines = StringSplitter.SplitIntoLines(text, out mRemainingText);
                                    foreach (string line in lines)
                                    {
                                        if (cancelToken.IsCancellationRequested) break;

                                        // Dispatch message if empty lne
                                        if (string.IsNullOrEmpty(line.Trim()) && mSse != null)
                                        {
                                            mLogger.LogDebug("Message received");
                                            msgReceived(mSse);
                                            mSse = null;
                                        }
                                        else if (line.StartsWith(":"))
                                        {
                                            mLogger.LogDebug("A comment was received: {line}", line);
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
                                                mSse = mSse ?? new ServerSentEvent();
                                                mSse.EventType = fieldValue;
                                            }
                                            else if (String.Compare(fieldName, "data", true) == 0)
                                            {
                                                mSse = mSse ?? new ServerSentEvent();
                                                mSse.Data = fieldValue + '\n';
                                            }
                                            else if (String.Compare(fieldName, "id", true) == 0)
                                            {
                                                mSse = mSse ?? new ServerSentEvent();
                                                mSse.LastEventId = fieldValue;
                                            }
                                            else if (String.Compare(fieldName, "retry", true) == 0)
                                            {
                                                int parsedRetry;
                                                if (int.TryParse(fieldValue, out parsedRetry))
                                                {
                                                    mSse = mSse ?? new ServerSentEvent();
                                                    mSse.Retry = parsedRetry;
                                                }
                                            }
                                            else
                                            {
                                                mLogger.LogInformation("An unknown line was received: {line}", line);
                                            }
                                        }
                                    }

                                    if (!cancelToken.IsCancellationRequested)
                                        return this;
                                }
                                else // end of the stream reached
                                {
                                    mLogger.LogDebug("No bytes read. End of stream.");
                                }
                            }
                            catch (Exception ex)
                            {
                                mLogger.LogInformation(ex, "ConnectedState.Run");
                            }
                        }

                        //stream.Dispose()
                        //stream.Close();
                        //mResponse.Close();
                        //mResponse.Dispose();
                        return new DisconnectedState(mResponse.ResponseUri, mWebRequesterFactory, mHeaders, mTokenService, mLogger);
                    }
                }
            });

            t.Start();
            return t;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Fint.Sse
{
    public class FintSseApplication
    {
        private readonly IFintEventListener _fintEventListener;
        private readonly FintSseSettings _fintSettings;

        public readonly List<EventSource> EventSources = new List<EventSource>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public FintSseApplication(
            IFintEventListener fintEventListener,
            IOptions<FintSseSettings> fintSettings)
        {
            _fintEventListener = fintEventListener;
            _fintSettings = fintSettings.Value;
        }

        public void Connect(string orgId)
        {
            _fintEventListener.Listen(orgId);

            if (_fintSettings.AllowConcurrentConnections)
            {
                Task.Run(async delegate
                {
                    int fintOptionsSseThreadInterval;
                    if (_fintSettings.SseThreadInterval > 0)
                    {
                        fintOptionsSseThreadInterval = _fintSettings.SseThreadInterval;
                    }
                    else
                    {
                        fintOptionsSseThreadInterval = Convert.ToInt32(TimeSpan.FromMinutes(10).TotalMilliseconds);
                    }

                    await Task.Delay(fintOptionsSseThreadInterval, _cts.Token)
                        .ContinueWith(_ => _fintEventListener.Listen(orgId), _cts.Token);

                }, _cts.Token);
            }
        }
    }
}

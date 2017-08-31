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
                    var fintOptionsSseThreadInterval = ThreadDelay;

                    await Task.Delay(fintOptionsSseThreadInterval, _cts.Token)
                        .ContinueWith(_ => _fintEventListener.Listen(orgId), _cts.Token);
                }, _cts.Token);
            }
        }

        private int ThreadDelay => _fintSettings.SseThreadIntervalInMinutes > 0 ? ConvertMinutesToMilliseconds(_fintSettings.SseThreadIntervalInMinutes) : ConvertMinutesToMilliseconds(10);

        private int ConvertMinutesToMilliseconds(int intervalInMinutes)
        {
            return Convert.ToInt32(TimeSpan.FromMinutes(intervalInMinutes).TotalMilliseconds);
        }

        public void Disconnect()
        {
            _fintEventListener.Disconnect();
        }
    }
}

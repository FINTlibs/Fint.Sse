using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fint.Event.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fint.Sse
{
    public class FintEventListener : IFintEventListener
    {
        private readonly int MAX_UUIDS = 50;
        private readonly ConcurrentBag<string> _organisationIdList = new ConcurrentBag<string>();
        private readonly ConcurrentBag<string> _uuids = new ConcurrentBag<string>();
        private EventSource _eventSource;
        private readonly FintSseSettings _appSettings;
        private readonly IEventHandler _eventHandler;
        private readonly ILogger<FintEventListener> _logger;
        private object lockObject = new object();

        public FintEventListener(
            IOptions<FintSseSettings> fintSettings,
            IEventHandler eventHandler,
            ILogger<FintEventListener> logger)
        {
            _appSettings = fintSettings.Value;
            _eventHandler = eventHandler;
            _logger = logger;
        }

        public void Listen(string orgId)
        {
            var headers = new Dictionary<string, string>
            {
                {FintHeaders.ORG_ID_HEADER, orgId}
            };

            var uuid = Guid.NewGuid().ToString();
            var url = new Uri(string.Format("{0}/{1}", _appSettings.SseEndpoint, uuid));

            if (!ContainsOrganisationId(orgId))
            {
                _organisationIdList.Add(orgId);
            }

            /*
             This seems wrong. We should add an event source per thread not reuse the same one...
             */
            _eventSource = new EventSource(url, headers, 10000);

            _eventSource.StateChanged += (o, e) =>
            {
                _logger.LogInformation("{orgId}: SSE state change {@state} for uuid {uuid}", orgId, e.State, uuid);
            };

            _eventSource.EventReceived += (o, e) =>
            {
                if (e?.Message != null)
                {
                    OnEventReceived(e.Message);
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            _eventSource.Start(cancellationTokenSource.Token);
            _eventSource.CancellationToken = cancellationTokenSource;
        }

        public void OnEventReceived(ServerSentEvent sse)
        {
            var serverSentEvent = EventUtil.ToEvent<object>(sse.Data);

            if (serverSentEvent != null)
            {
                if (IsNewCorrId(serverSentEvent.CorrId))
                {
                    if (ContainsOrganisationId(serverSentEvent.OrgId))
                    {
                        _logger.LogInformation("{orgId}: Event received {@Event}", serverSentEvent.OrgId,
                            serverSentEvent.Data);
                        _eventHandler.HandleEvent(serverSentEvent);
                    }
                    else
                    {
                        _logger.LogInformation("This is not EventListener for {org}", serverSentEvent.OrgId);
                    }
                }
                else
                {
                    _logger.LogInformation("This EventListener has already started processing {corrId} for {ordgID}",
                        serverSentEvent.CorrId, serverSentEvent.OrgId);
                }
            }
            else
            {
                _logger.LogError("Could not parse Event object {data}", sse.Data);
            }
        }

        private bool IsNewCorrId(string corrId)
        {
            lock (lockObject)
            {
                if (_uuids.Contains(corrId))
                {
                    return false;
                }

                if (_uuids.Count >= MAX_UUIDS)
                {
                    _uuids.First().Remove(0);
                }

                _uuids.Add(corrId);

                return true;
            }
        }

        private bool ContainsOrganisationId(string orgId)
        {
            lock (lockObject)
            {
                return _organisationIdList != null && _organisationIdList.Contains(orgId);
            }
        }
    }
}
﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private ITokenService _tokenService;
        private readonly object _lockObject = new object();

        public FintEventListener(
            IOptions<FintSseSettings> fintSettings,            
            IEventHandler eventHandler,
            ILogger<FintEventListener> logger, ITokenService tokenService)
        {
            _appSettings = fintSettings.Value;            
            _eventHandler = eventHandler;
            _logger = logger;
            _tokenService = tokenService;            
        }
 
        public void Listen(string orgId)
        {
            if (!ContainsOrganisationId(orgId))
            {
                _organisationIdList.Add(orgId);
            }

            var uuid = Guid.NewGuid().ToString();
            _logger.LogInformation("SSE client id: {uuid}", uuid);
            var url = new Uri(string.Format("{0}/{1}", _appSettings.SseEndpoint, uuid));

            if (!ContainsOrganisationId(orgId))
            {
                _organisationIdList.Add(orgId);
            }                       
            var headers = new Dictionary<string, string>
            {
                {FintHeaders.ORG_ID_HEADER, orgId}
            };

            _eventSource = new EventSource(url, headers, 10000, _tokenService, _logger);

            _eventSource.StateChanged += (o, e) =>
            {                
                _logger.LogDebug("{orgId}: SSE state change {@state} for uuid {uuid}", orgId, e.State, uuid);
            };

            _eventSource.EventReceived += (o, e) =>
            {
                if (e?.Message?.Data != null)
                {
                    OnEventReceived(e.Message);
                }
            };

            var cancellationTokenSource = new CancellationTokenSource();
            _eventSource.Start(cancellationTokenSource.Token);
            _eventSource.CancellationToken = cancellationTokenSource;
        }

        public void Disconnect()
        {
            _logger.LogInformation("Stop listening to {eventSource}", _eventSource.Url);
            _eventSource.CancellationToken.Cancel();
        }

        public void OnEventReceived(ServerSentEvent sse)
        {
            var serverSentEvent = EventUtil.ToEvent<object>(sse.Data);

            if (serverSentEvent == null)
            {
                _logger.LogError("Could not parse Event object {data}", sse.Data);
                return;
            }

            if (!IsNewCorrId(serverSentEvent.CorrId))
            {
                _logger.LogTrace("This EventListener has already started processing {corrId} for {ordgID}", serverSentEvent.CorrId, serverSentEvent.OrgId);
                return;
            }

            if (!ContainsOrganisationId(serverSentEvent.OrgId))
            {
                _logger.LogInformation("Received event {corrId} from {source} for {org}", 
                    serverSentEvent.CorrId,
                    serverSentEvent.Source,
                    serverSentEvent.OrgId);
                return;
            }

            _logger.LogInformation("{orgId}: Event received from {@Source}: {@Event}", serverSentEvent.OrgId, serverSentEvent.Source, serverSentEvent.Action);
            // var accessToken = _tokenClient.AccessToken;
            _eventHandler.HandleEvent(serverSentEvent);
        }

        private bool IsNewCorrId(string corrId)
        {
            lock (_lockObject)
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
            lock (_lockObject)
            {
                return _organisationIdList != null && _organisationIdList.Contains(orgId);
            }
        }        
    }
}
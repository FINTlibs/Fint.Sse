namespace Fint.Sse
{
    public class FintSseSettings
    {
        public int SseThreadIntervalInMinutes { get; set; }
        public bool AllowConcurrentConnections { get; set; }
        public string[] Organizations { get; set; }
        public SseEndpoint[] SseEndpoints { get; set; }        
    }

    public class SseEndpoint {
        public string SseUri { get; set; }
        public string StatusUri { get; set; }
        public string ResponseUri { get; set; }
    }
}

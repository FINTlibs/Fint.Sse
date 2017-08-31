namespace Fint.Sse
{
    public class FintSseSettings
    {
        public int SseThreadIntervalInMinutes { get; set; }
        public bool AllowConcurrentConnections { get; set; }
        public string[] Organizations { get; set; }
        public object SseEndpoint { get; set; }
    }
}

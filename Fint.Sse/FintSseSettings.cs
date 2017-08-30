using System;
using System.Collections.Generic;
using System.Text;

namespace Fint.Sse
{
    public class FintSseSettings
    {
        public int SseThreadInterval { get; set; }
        public bool AllowConcurrentConnections { get; set; } = true;
        public string[] Organizations { get; set; }
        public object SseEndpoint { get; set; } = "https://play-with-fint-adapter.felleskomponent.no/provider/sse";
    }
}

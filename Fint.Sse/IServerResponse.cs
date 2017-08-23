using System;
using System.Net;

namespace Fint.Sse
{
    public interface IServerResponse
    {
        HttpStatusCode StatusCode { get; }

        System.IO.Stream GetResponseStream();

        Uri ResponseUri { get; }
    }
}

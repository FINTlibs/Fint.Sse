using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fint.Sse
{
    public interface IWebRequester
    {
        Task<IServerResponse> Get(Uri url, Dictionary<string, string> headers = null);
    }
}

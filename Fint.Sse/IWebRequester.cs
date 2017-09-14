using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fint.Sse
{
    public interface IWebRequester
    {
        Task<IServerResponse> Get(Uri url, ITokenService tokenService, Dictionary<string, string> headers = null);
    }
}

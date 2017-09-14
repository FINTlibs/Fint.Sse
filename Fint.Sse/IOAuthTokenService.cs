using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace Fint.Sse
{
    public interface IOAuthTokenService
    {
        Task<string> GetAccessTokenAsync();
        bool UseAuthentication { get; }
    }
}

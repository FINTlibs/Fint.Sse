using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fint.Sse
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
        bool UseAuthentication { get; }
    }
}

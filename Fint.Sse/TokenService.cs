using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fint.Sse
{
    public class TokenService : ITokenService
    {
        private IOAuthTokenService _tokenClient;
        public bool OAuthEnabled { get { return _tokenClient.OAuthEnabled; } }        

        public TokenService(IOAuthTokenService tokenClient)
        {
            _tokenClient = tokenClient;
        }        

        public async Task<string> GetAccessTokenAsync()
        {
            return await _tokenClient.GetAccessTokenAsync();
        }        
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fint.Sse
{
    public class TokenService : ITokenService
    {
        private IOAuthTokenService mTokenClient;
        public bool OAuthEnabled { get { return mTokenClient.OAuthEnabled; } }        

        public TokenService(IOAuthTokenService tokenClient)
        {
            mTokenClient = tokenClient;
        }        

        public async Task<string> GetAccessTokenAsync()
        {
            return await mTokenClient.GetAccessTokenAsync();
        }        
    }
}

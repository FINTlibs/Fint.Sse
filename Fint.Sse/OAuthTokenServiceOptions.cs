using System;
using System.Collections.Generic;
using System.Text;

namespace Fint.Sse
{
    public class OAuthTokenServiceOptions
    {
        public string TokenEndpoint { get; set; }
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
        public bool UseAuthentication { get; set; }
    }
}

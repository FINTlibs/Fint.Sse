using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using IdentityModel.Client;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Fint.Sse
{    

    public class OAuthTokenService : IOAuthTokenService
    {
        private TokenClient mTokenClient;
        private HttpClient mHttpClient;

        private string mAccessToken { get; set; }
        private string mRefreshToken { get; set; }
        private long mExpiresIn { get; set; }
        private string mTokenEndpoint { get; set; }
        private string mClientId { get; set; }
        private string mSecret { get; set; }
        private string mUsername { get; set; }
        private string mPassword { get; set; }
        private string mScope { get; set; }
        private DateTime mExpiresAt { get; set; }
        public bool UseAuthentication { get; }
        

        public OAuthTokenService(IOptions<OAuthTokenServiceOptions> options, HttpClient httpClient)
        {
            UseAuthentication = options.Value.UseAuthentication;

            if (UseAuthentication)
            {
                if (options.Value.TokenEndpoint == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Token Endpoint can't be empty or null");
                if (options.Value.ClientId == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Client Id can't be empty or null");
                if (options.Value.Secret == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Client secret can't be empty or null");
                if (options.Value.Username == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Username can't be empty or null");
                if (options.Value.Password == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Password can't be empty or null");
                if (options.Value.Scope == String.Empty || options.Value.TokenEndpoint == null) throw new ArgumentNullException("Scope can't be empty or null");
                
                mTokenEndpoint = options.Value.TokenEndpoint;
                mClientId = options.Value.ClientId;
                mSecret = options.Value.Secret;
                mUsername = options.Value.Username;
                mPassword = options.Value.Password;
                mScope = options.Value.Scope;

                mTokenClient = new TokenClient(
                    mTokenEndpoint,
                    mClientId,
                    mSecret);

                mHttpClient = httpClient;
            }
        }        

        public async Task<string> GetAccessTokenAsync()
        {            
            if (mAccessToken == null)
            {
                var response = await RequestAccessTokenAsync();
                mAccessToken = response.AccessToken;
                mRefreshToken = response.RefreshToken;
                mExpiresIn = response.ExpiresIn;
                mExpiresAt = DateTime.UtcNow.AddSeconds(mExpiresIn).ToLocalTime();
            }
            else
            {
                if (mExpiresAt < DateTime.UtcNow)
                {
                    var response = await RefreshTokenAsync(mRefreshToken);
                    mAccessToken = response.AccessToken;                    
                    mExpiresIn = response.ExpiresIn;
                    mExpiresAt = DateTime.UtcNow.AddSeconds(mExpiresIn).ToLocalTime();
                }
            }
                        
            return mAccessToken;            
        }         
        
        private async Task<TokenResponse> RequestAccessTokenAsync()
        {                                    
            return await mTokenClient.RequestResourceOwnerPasswordAsync(mUsername, mPassword, mScope);
        }

        private async Task<RefreshToken> RefreshTokenAsync(string refreshToken)
        {            
            var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(mClientId + ":" + mSecret));            
            mHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);

            var form = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };            

            var url = mTokenEndpoint + "?client_id=" + mClientId + "&client_secret=" + mSecret + "&scope=" + mScope;            
            var response = await mHttpClient.PostAsync(url, new FormUrlEncodedContent(form));
            var jsonSerializer = new DataContractJsonSerializer(typeof(RefreshToken));
            var responseStream = await response.Content.ReadAsStreamAsync();
            var token = (RefreshToken)jsonSerializer.ReadObject(responseStream);            
            return token;
        }
    }

    [DataContract]
    internal class RefreshToken
    {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "expires_in")]
        public long ExpiresIn { get; set; }

        [DataMember(Name = "scope")]
        public string scope { get; set; }
    }
    
}

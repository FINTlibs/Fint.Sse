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
        private string mAccessTokenUri { get; set; }
        private string mClientId { get; set; }
        private string mClientSecret { get; set; }
        private string mUsername { get; set; }
        private string mPassword { get; set; }
        private string mScope { get; set; }
        private DateTime mExpiresAt { get; set; }
        public bool OAuthEnabled { get; }
        

        public OAuthTokenService(IOptions<OAuthTokenServiceOptions> options, HttpClient httpClient)
        {
            OAuthEnabled = options.Value.OAuthEnabled;

            if (OAuthEnabled)
            {
                if (options.Value.AccessTokenUri == String.Empty || options.Value.AccessTokenUri == null) throw new ArgumentNullException("Token Endpoint can't be empty or null");
                if (options.Value.ClientId == String.Empty || options.Value.ClientId == null) throw new ArgumentNullException("Client Id can't be empty or null");
                if (options.Value.ClientSecret == String.Empty || options.Value.ClientSecret == null) throw new ArgumentNullException("Client ClientSecret can't be empty or null");
                if (options.Value.Username == String.Empty || options.Value.Username == null) throw new ArgumentNullException("Username can't be empty or null");
                if (options.Value.Password == String.Empty || options.Value.Password == null) throw new ArgumentNullException("Password can't be empty or null");
                if (options.Value.Scope == String.Empty || options.Value.Scope == null) throw new ArgumentNullException("Scope can't be empty or null");
                
                mAccessTokenUri = options.Value.AccessTokenUri;
                mClientId = options.Value.ClientId;
                mClientSecret = options.Value.ClientSecret;
                mUsername = options.Value.Username;
                mPassword = options.Value.Password;
                mScope = options.Value.Scope;

                mTokenClient = new TokenClient(
                    mAccessTokenUri,
                    mClientId,
                    mClientSecret);

                mHttpClient = httpClient;
            }
        }        

        public async Task<string> GetAccessTokenAsync()
        {
            // Add extra time 5 minutes to ensure the token has not expired before we can use it
            var currentTime = DateTime.UtcNow.AddMinutes(5).ToLocalTime();
            var expired = mExpiresAt < currentTime;
            if (mAccessToken == null || expired)
            { 
               if (mRefreshToken == null)
                {
                    //Console.WriteLine("Getting Access Token from " + mAccessTokenUri);
                    var response = await RequestAccessTokenAsync();
                    if (response.IsError) throw new Exception("OAuth Access Token error: " + response.Error + ", " + response.ErrorDescription);
                    mAccessToken = response.AccessToken;
                    mRefreshToken = response.RefreshToken;
                    mExpiresIn = response.ExpiresIn;
                    mExpiresAt = DateTime.UtcNow.AddSeconds(mExpiresIn).ToLocalTime();
                }
                else
                {
                    //Console.WriteLine("Getting Refresh Token from " + mAccessTokenUri);
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
            // TODO: use mTokenClient.RequestRefreshTokenAsync()
            var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(mClientId + ":" + mClientSecret));            
            mHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);

            var form = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };            

            var url = mAccessTokenUri + "?client_id=" + mClientId + "&client_secret=" + mClientSecret + "&scope=" + mScope;            
            var response = await mHttpClient.PostAsync(url, new FormUrlEncodedContent(form));
            if (!response.IsSuccessStatusCode) throw new Exception("OAuth Refresh Token error: " + response.ReasonPhrase);
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

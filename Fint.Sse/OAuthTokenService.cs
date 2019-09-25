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
        private TokenClient _tokenClient;
        private HttpClient _httpClient;

        private string _accessToken { get; set; }
        private string _refreshToken { get; set; }
        private long _expiresIn { get; set; }
        private string _accessTokenUri { get; set; }
        private string _clientId { get; set; }
        private string _clientSecret { get; set; }
        private string _username { get; set; }
        private string _password { get; set; }
        private string _scope { get; set; }
        private DateTime _expiresAt { get; set; }
        public bool OAuthEnabled { get; }
        

        public OAuthTokenService(IOptions<OAuthTokenServiceOptions> options, HttpClient httpClient)
        {
            OAuthEnabled = options.Value.OAuthEnabled;

            if (OAuthEnabled)
            {
                if (string.IsNullOrEmpty(options.Value.AccessTokenUri)) throw new ArgumentNullException("Token Endpoint can't be empty or null");
                if (string.IsNullOrEmpty(options.Value.ClientId)) throw new ArgumentNullException("Client Id can't be empty or null");
                if (string.IsNullOrEmpty(options.Value.ClientSecret)) throw new ArgumentNullException("Client ClientSecret can't be empty or null");
                if (string.IsNullOrEmpty(options.Value.Username)) throw new ArgumentNullException("Username can't be empty or null");
                if (string.IsNullOrEmpty(options.Value.Password)) throw new ArgumentNullException("Password can't be empty or null");
                if (string.IsNullOrEmpty(options.Value.Scope)) throw new ArgumentNullException("Scope can't be empty or null");
                
                _accessTokenUri = options.Value.AccessTokenUri;
                _clientId = options.Value.ClientId;
                _clientSecret = options.Value.ClientSecret;
                _username = options.Value.Username;
                _password = options.Value.Password;
                _scope = options.Value.Scope;

                _tokenClient = new TokenClient(
                    _accessTokenUri,
                    _clientId,
                    _clientSecret);

                _httpClient = httpClient;
            }
        }        

        public async Task<string> GetAccessTokenAsync()
        {
            // Add extra time 5 minutes to ensure the token has not expired before we can use it
            var currentTime = DateTime.UtcNow.AddMinutes(5).ToLocalTime();
            var expired = _expiresAt < currentTime;
            if (_accessToken == null || expired)
            { 
               if (_refreshToken == null)
                {
                    //Console.WriteLine("Getting Access Token from " + mAccessTokenUri);
                    var response = await RequestAccessTokenAsync();
                    if (response.IsError) throw new Exception("OAuth Access Token error: " + response.Error + ", " + response.ErrorDescription);
                    _accessToken = response.AccessToken;
                    _refreshToken = response.RefreshToken;
                    _expiresIn = response.ExpiresIn;
                    _expiresAt = DateTime.UtcNow.AddSeconds(_expiresIn).ToLocalTime();
                }
                else
                {
                    //Console.WriteLine("Getting Refresh Token from " + mAccessTokenUri);
                    var response = await RefreshTokenAsync(_refreshToken);
                    _accessToken = response.AccessToken;                    
                    _expiresIn = response.ExpiresIn;
                    _expiresAt = DateTime.UtcNow.AddSeconds(_expiresIn).ToLocalTime();
                }
            }
            return _accessToken;            
        }         
        
        private async Task<TokenResponse> RequestAccessTokenAsync()
        {                                    
            return await _tokenClient.RequestResourceOwnerPasswordAsync(_username, _password, _scope);
        }

        private async Task<RefreshToken> RefreshTokenAsync(string refreshToken)
        {            
            // TODO: use mTokenClient.RequestRefreshTokenAsync()
            var authorizationHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _clientSecret));            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorizationHeader);

            var form = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken}
            };            

            var url = _accessTokenUri + "?client_id=" + _clientId + "&client_secret=" + _clientSecret + "&scope=" + _scope;            
            var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(form));
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

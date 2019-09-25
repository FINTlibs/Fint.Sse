using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Fint.Sse
{
    class WebRequester : IWebRequester
    {
        public Task<IServerResponse> Get(Uri url, ITokenService tokenService, Dictionary<string, string> headers = null)
        {
            var accessToken = "";

            if (tokenService.OAuthEnabled)
            {                
                var task = Task.Run(async () => await tokenService.GetAccessTokenAsync());

                accessToken = task.Result;                
            }
                     
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Proxy = null; 

            if (tokenService.OAuthEnabled)
            {
                webRequest.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
            }
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }
            }

            var taskResp = Task.Factory.FromAsync<WebResponse>(webRequest.BeginGetResponse,
                                                            webRequest.EndGetResponse,
                                                            null).ContinueWith<IServerResponse>(t => new ServerResponse(t.Result));
            return taskResp;

        }
    }
}

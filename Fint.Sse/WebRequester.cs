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

            if (tokenService.UseAuthentication)
            {                
                var task = Task.Run(async () => {
                    return await tokenService.GetAccessTokenAsync();
                });

                accessToken = task.Result;                
            }
                     
            var wreq = (HttpWebRequest)WebRequest.Create(url);
            wreq.Method = "GET";
            wreq.Proxy = null; 

            if (tokenService.UseAuthentication)
            {
                wreq.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
            }
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    wreq.Headers.Add(header.Key, header.Value);
                }
            }

            var taskResp = Task.Factory.FromAsync<WebResponse>(wreq.BeginGetResponse,
                                                            wreq.EndGetResponse,
                                                            null).ContinueWith<IServerResponse>(t => new ServerResponse(t.Result));
            return taskResp;

        }
    }
}

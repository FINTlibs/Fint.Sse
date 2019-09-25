using System;
using System.Net;

namespace Fint.Sse
{
    class ServerResponse : IServerResponse
    {
        private System.Net.HttpWebResponse _httpResponse;

        public ServerResponse(System.Net.WebResponse webResponse)
        {
            this._httpResponse = webResponse as HttpWebResponse;
        }

        public HttpStatusCode StatusCode => _httpResponse.StatusCode;

        public System.IO.Stream GetResponseStream()
        {
            return _httpResponse.GetResponseStream();
        }

        public Uri ResponseUri => _httpResponse.ResponseUri;
    }
}

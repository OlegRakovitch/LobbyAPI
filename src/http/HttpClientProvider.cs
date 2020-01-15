using System.Net.Http;

namespace RattusAPI.Http
{
    public class HttpClientProvider : HttpClient, IHttpClientProvider
    {
        public string RegisteredName => "HttpClient";
    }
}
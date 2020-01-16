using System.Net.Http;

namespace LobbyAPI.Http
{
    public class HttpClientProvider : HttpClient, IHttpClientProvider
    {
        public string RegisteredName => "HttpClient";
    }
}
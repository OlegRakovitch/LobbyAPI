using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LobbyAPI.Http;

namespace LobbyAPI.Tests
{
    public class InternalHttpClientProvider : IHttpClientProvider
    {
        public string RegisteredName => "Internal";
        Dictionary<string, string> registeredResponses = new Dictionary<string, string>();

        public void RegisterResponse(string requestUri, string response)
        {
            registeredResponses.Add(requestUri, response);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = registeredResponses[request.RequestUri.OriginalString];
            var responseMessage = new HttpResponseMessage();
            responseMessage.Content = new StringContent(response);
            responseMessage.StatusCode = System.Net.HttpStatusCode.OK;
            return Task.FromResult(responseMessage);
        }
    }
}
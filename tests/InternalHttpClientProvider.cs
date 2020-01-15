using System.Net.Http;
using System.Threading.Tasks;
using RattusAPI.Http;

namespace RattusAPI.Tests
{
    public class InternalHttpClientProvider : IHttpClientProvider
    {
        public string RegisteredName => "Internal";
        HttpClient client;

        public InternalHttpClientProvider(HttpClient client)
        {
            this.client = client;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await client.SendAsync(request);
        }
    }
}
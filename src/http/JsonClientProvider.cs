using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LobbyAPI.Http
{
    public class JsonClientProvider : ISerializedHttpClientProvider
    {
        public string RegisteredName => "Json";

        IHttpClientProvider client;

        public JsonClientProvider(IHttpClientProvider provider)
        {
            client = provider;
        }

        public async Task<T> SendAsync<T>(HttpMethod method, string uri, object content = null)
        {
            using (var requestMessage = new HttpRequestMessage(method, uri))
            {
                if (content != null)
                {
                    var serialized = JsonConvert.SerializeObject(content);
                    requestMessage.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
                }
                var c = new HttpClient();
                var response = client.SendAsync(requestMessage);
                var text = await response.Result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(text);
            }
        }
    }
}
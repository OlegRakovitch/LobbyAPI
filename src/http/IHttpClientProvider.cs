using System.Net.Http;
using System.Threading.Tasks;
using LobbyAPI.Provider;

namespace LobbyAPI.Http
{
    public interface IHttpClientProvider : IProvider
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
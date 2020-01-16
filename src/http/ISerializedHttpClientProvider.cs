using System.Net.Http;
using System.Threading.Tasks;
using LobbyAPI.Provider;

namespace LobbyAPI.Http
{
    public interface ISerializedHttpClientProvider : IProvider
    {
        Task<T> SendAsync<T>(HttpMethod method, string uri, object content = null);
    }
}
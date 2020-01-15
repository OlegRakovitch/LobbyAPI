using System.Net.Http;
using System.Threading.Tasks;
using RattusAPI.Provider;

namespace RattusAPI.Http
{
    public interface IHttpClientProvider : IProvider
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
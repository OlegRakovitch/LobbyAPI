using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RattusAPI.Tests
{
    public class RequestHelper
    {
        readonly HttpClient Client;
        public RequestHelper(HttpClient client)
        {
            Client = client;
        }

        public AuthenticationContext GetAuthenticationContext(string username)
        {
            return AuthenticationContext.Create(username);
        }

        public async Task<HttpResponseMessage> SendAsync(AuthenticationContext context, HttpMethod method, string path, string content = null)
        {
            using (var requestMessage = new HttpRequestMessage(method, path))
            {
                if (!string.IsNullOrEmpty(context.Username))
                {
                    requestMessage.Headers.Add("username", context.Username);
                }
                if (!string.IsNullOrEmpty(content))
                {
                    requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
                }
                return await Client.SendAsync(requestMessage);
            }
        }
    }
}
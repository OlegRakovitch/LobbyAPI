using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RattusAPI.Tests
{
    public class AdminControllerTests : IClassFixture<AppFactory<TestsStartup>>
    {
        readonly RequestHelper helper;

        public AdminControllerTests(AppFactory<TestsStartup> factory)
        {
            var client = factory.UseOriginalStartup().CreateClient();
            helper = new RequestHelper(client);
        }

        [Fact]
        public async Task UserCanNotResetAnonymously()
        {
            var context = AuthenticationContext.Empty;
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/admin/reset");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UserCanNotReset()
        {
            var context = helper.GetAuthenticationContext("user");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/admin/reset");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AdminCanReset()
        {
            var context = helper.GetAuthenticationContext("admin");
            var response = await helper.SendAsync(context, HttpMethod.Post, "/api/admin/reset");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
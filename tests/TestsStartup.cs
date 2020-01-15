using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RattusAPI.Tests
{
    public class TestsStartup : Startup
    {
        public TestsStartup(IConfiguration configuration) : base(configuration)
        {
            
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }
    }
}
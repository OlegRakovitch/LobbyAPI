using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace LobbyAPI.Tests
{
    public class AppFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
    {

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(null).UseStartup<TEntryPoint>();
        }

        protected override IEnumerable<Assembly> GetTestAssemblies()
        {
            return new Assembly[] { typeof(TEntryPoint).Assembly };
        }
    }

    public static class AppFactoryExtensions
    {
        public static WebApplicationFactory<T> UseOriginalStartup<T>(this WebApplicationFactory<T> factory) where T: class
        {
            return factory.WithWebHostBuilder(builder =>
            {  
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<System.Net.Http.HttpClient>(factory.CreateClient());
                    services.AddMvc().AddApplicationPart(typeof(LobbyAPI.Startup).Assembly);
                    var httpClient = services.BuildServiceProvider().GetService<System.Net.Http.HttpClient>();
                });
            });
        }
    }
}
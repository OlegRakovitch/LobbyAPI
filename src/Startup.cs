using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RattusAPI.Authentication;
using RattusAPI.Configuration;
using RattusAPI.Context;
using RattusAPI.GameStarter;
using RattusAPI.Http;
using RattusAPI.LobbyEngine;
using RattusAPI.Provider;
using RattusAPI.Storage;

namespace RattusAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => {
                options.Filters.Add<AuthenticationFilter>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpContextAccessor();

            var providers = Configuration.GetSection("Providers");

            services
                .AddProviderFactory<IAuthenticationProvider>(providers.Value("Authentication:Name"))
                .AddProviderFactory<IStorageProvider>(providers.Value("Storage:Name"))
                .AddProviderFactory<IHttpClientProvider>(providers.Value("HttpClient:Name"))
                .AddProviderFactory<ISerializedHttpClientProvider>(providers.Value("SerializedHttpClient:Name"))
                .AddProviderFactory<IContextProvider>(providers.Value("Context:Name"))
                .AddProviderFactory<IGameStarterProvider>(providers.Value("GameStarter:Name"))
                .AddProviderFactory<ILobbyEngineProvider>(providers.Value("LobbyEngine:Name"));
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            app.UseMvc();
        }
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LobbyAPI.Authentication;
using LobbyAPI.Configuration;
using LobbyAPI.Context;
using LobbyAPI.GameStarter;
using LobbyAPI.Http;
using LobbyAPI.Lobby;
using LobbyAPI.Provider;
using LobbyAPI.Storage;

namespace LobbyAPI
{
    public class Startup
    {
        readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => {
                options.Filters.Add<AuthenticationFilter>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddHttpContextAccessor();

            services.CreateFactoryBuilder(configuration)
                .AddProviderFactory<IAuthenticationProvider>("Authentication")
                .AddProviderFactory<IStorageProvider>("Storage")
                .AddProviderFactory<IHttpClientProvider>("HttpClient")
                .AddProviderFactory<ISerializedHttpClientProvider>("SerializedHttpClient")
                .AddProviderFactory<IContextProvider>("Context")
                .AddProviderFactory<IGameStarterProvider>("GameStarter")
                .AddProviderFactory<ILobbyEngineProvider>("LobbyEngine");
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

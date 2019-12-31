using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RattusAPI.Authentication;
using RattusAPI.Factories;
using RattusEngine;

namespace RattusAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => {
                options.Filters.Add<AuthenticationFilter>();
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHttpContextAccessor();
            services.AddSingleton(typeof(IAuthentication), AuthenticationFactory.Find(Configuration["Authentication"]));

            services.AddSingleton(typeof(IStorage), StorageFactory.Find(Configuration["Storage"]));
            services.AddSingleton<IContext, ApplicationContext>();
            services.AddSingleton<IApplication, Application>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

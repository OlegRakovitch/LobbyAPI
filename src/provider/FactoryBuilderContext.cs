using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LobbyAPI.Provider
{
    public class FactoryBuilderContext
    {
        readonly IServiceCollection services;
        readonly IConfigurationSection configuration;

        static Lazy<Assembly[]> assemblies = new Lazy<Assembly[]>(() => AppDomain.CurrentDomain.GetAssemblies());

        static Type[] GetProviderImplementations<TProvider>() where TProvider: class
        {
            return assemblies.Value.SelectMany(assembly => assembly.GetTypes().Where(type => !type.IsAbstract && typeof(TProvider).IsAssignableFrom(type))).ToArray();
        }

        public FactoryBuilderContext(IServiceCollection services, IConfiguration configuration)
        {
            this.services = services;
            this.configuration = configuration.GetSection("Providers");
        }        

        public FactoryBuilderContext AddProviderFactory<TProvider>(string configSection = "") where TProvider : class, IProvider
        {
            var section = configuration.GetSection(configSection);
            var providerConfiguration = new ProviderConfiguration<TProvider>(section.GetSection("Properties"));
            services.AddSingleton<IProviderConfiguration<TProvider>>(providerConfiguration);
            
            var implementations = GetProviderImplementations<TProvider>();
            foreach(var implementation in implementations)
            {
                services.AddSingleton(implementation);
            }

            services.AddSingleton<ProviderFactory<TProvider>>(serviceProvider => {
                var factory = new ProviderFactory<TProvider>();
                foreach(var implementation in implementations)
                {
                    var service = serviceProvider.GetRequiredService(implementation) as TProvider;
                    factory.Register(service.RegisteredName, service);
                }
                return factory;
            });

            if (section != null)
            {
                services.AddSingleton<TProvider>(serviceProvider => {
                    var defaultProvider = section.GetValue<string>("Default");
                    var factory = serviceProvider.GetRequiredService<ProviderFactory<TProvider>>();
                    return factory.Create(defaultProvider); 
                });
            }

            return this;
        }
    }
}
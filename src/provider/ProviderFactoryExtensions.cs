using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LobbyAPI.Provider
{
    public static class ProviderFactoryExtensions
    {
        static Lazy<Assembly[]> assemblies = new Lazy<Assembly[]>(() => AppDomain.CurrentDomain.GetAssemblies());

        static Type[] GetProviderImplementations<TProvider>() where TProvider: class
        {
            return assemblies.Value.SelectMany(assembly => assembly.GetTypes().Where(type => !type.IsAbstract && typeof(TProvider).IsAssignableFrom(type))).ToArray();
        }

        public static IServiceCollection AddProviderFactory<TProvider>(this IServiceCollection services, string defaultProvider = "") where TProvider : class, IProvider
        {
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

            if (!string.IsNullOrEmpty(defaultProvider))
            {
                services.AddSingleton<TProvider>(serviceProvider => {
                    var factory = serviceProvider.GetRequiredService<ProviderFactory<TProvider>>();
                    return factory.Create(defaultProvider); 
                });
            }

            return services;
        }
    }
}
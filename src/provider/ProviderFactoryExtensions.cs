using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LobbyAPI.Provider
{
    public static class ProviderFactoryExtensions
    {
        public static FactoryBuilderContext CreateFactoryBuilder(this IServiceCollection services, IConfiguration configuration)
        {
            return new FactoryBuilderContext(services, configuration);
        }
    }
}
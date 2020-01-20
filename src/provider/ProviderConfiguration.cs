using Microsoft.Extensions.Configuration;

namespace LobbyAPI
{
    public class ProviderConfiguration<TProvider> : IProviderConfiguration<TProvider>
    {
        readonly IConfigurationSection properties;
        public ProviderConfiguration(IConfigurationSection section)
        {
            properties = section;
        }

        public string this[string key] => properties[key];
    }
}
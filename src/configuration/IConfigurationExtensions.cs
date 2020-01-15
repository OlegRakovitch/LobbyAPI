using Microsoft.Extensions.Configuration;

namespace RattusAPI.Configuration
{
    public static class IConfigurationExtensions
    {
        public static string Value(this IConfiguration configuration, string key)
        {
            return configuration.GetValue<string>(key);
        }
    }
}
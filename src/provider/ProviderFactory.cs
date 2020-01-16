using System;
using System.Collections.Generic;

namespace LobbyAPI.Provider
{
    public class ProviderFactory<TProvider>
    {
        Dictionary<string, TProvider> factory = new Dictionary<string, TProvider>();

        public TProvider Create(string authenticationTypeName)
        {
            if (factory.ContainsKey(authenticationTypeName))
            {
                return factory[authenticationTypeName];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void Register(string authenticationTypeName, TProvider provider)
        {
            factory.Add(authenticationTypeName, provider);
        }
    }
}
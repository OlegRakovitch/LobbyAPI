using System.Collections.Generic;

namespace LobbyAPI.Tests
{
    public class InternalConfiguration<TProvider> : IProviderConfiguration<TProvider>
    {
        Dictionary<string, string> dictionary;
        public InternalConfiguration(Dictionary<string, string> values)
        {
            this.dictionary = values;
        }
        public string this[string key] => dictionary[key];
    }
}
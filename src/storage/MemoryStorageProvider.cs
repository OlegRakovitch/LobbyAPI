using LobbyEngine;

namespace LobbyAPI.Storage
{
    public class MemoryStorageProvider : MemoryStorage, IStorageProvider
    {
        public string RegisteredName => "Memory";
    }
}
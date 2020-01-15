using RattusEngine;

namespace RattusAPI.Storage
{
    public class MemoryStorageProvider : MemoryStorage, IStorageProvider
    {
        public string RegisteredName => "Memory";
    }
}
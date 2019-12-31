using System;
using RattusEngine;

namespace RattusAPI.Factories
{
    public static class StorageFactory
    {
        public static Type Find(string storageTypeName)
        {
            switch(storageTypeName)
            {
                case "Memory":
                    return typeof(MemoryStorage);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
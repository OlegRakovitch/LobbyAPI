using Microsoft.AspNetCore.Http;
using RattusAPI.Storage;
using RattusEngine;
using RattusEngine.Models;

namespace RattusAPI.Context
{
    public class ContextProvider : IContextProvider
    {
        public string RegisteredName => "Authenticated";

        public IStorage Storage { get; private set; }
        readonly IHttpContextAccessor contextAccessor;

        public ContextProvider(IHttpContextAccessor accessor, IStorageProvider storage)
        {
            contextAccessor = accessor;
            Storage = storage;
        }

        public User GetUser()
        {
            var user = contextAccessor.HttpContext.User;
            if (user != null)
            {
                return new User()
                {
                    Username = user.Identity.Name,
                    Id = user.Identity.Name
                };
            }
            else
            {
                return null;
            }
        }
    }
}
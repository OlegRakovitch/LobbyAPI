using Microsoft.AspNetCore.Http;
using RattusEngine;
using RattusEngine.Models;

namespace RattusAPI
{
    public class ApplicationContext : IContext
    {
        public IStorage Storage { get; private set; }
        readonly IHttpContextAccessor contextAccessor;

        public ApplicationContext(IHttpContextAccessor accessor, IStorage storage)
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
using System.Linq;
using Microsoft.AspNetCore.Http;
using RattusEngine;
using RattusEngine.Models;

public class JWTContextRepository : IContext
{
    private readonly IHttpContextAccessor contextAccessor;

    public JWTContextRepository(IHttpContextAccessor accessor, IStorage storage)
    {
        contextAccessor = accessor;
        Storage = storage;
    }

    public IStorage Storage { get; private set; }

    public User GetUser()
    {
        var username = contextAccessor.HttpContext.User.Identity.Name;
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }
        else
        {
            return Storage.Get<User>().SingleOrDefault(user => user.Username == username);
        }
    }
}
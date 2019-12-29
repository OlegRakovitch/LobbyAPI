using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using RattusEngine;
using RattusEngine.Models;

public class TrustyContextRepository : IContext
{
    private readonly IHttpContextAccessor contextAccessor;

    public TrustyContextRepository(IHttpContextAccessor accessor, IStorage storage)
    {
        contextAccessor = accessor;
        Storage = storage;
    }

    public IStorage Storage { get; private set; }

    public User GetUser()
    {
        var header = contextAccessor.HttpContext.Request.Headers["username"];
        if (header == StringValues.Empty)
        {
            return null;
        }
        else
        {
            var username = header.Single();
            return new User
            {
                Id = username,
                Username = username
            };
        }
    }
}
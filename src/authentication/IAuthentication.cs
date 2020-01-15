using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using RattusAPI.Provider;

namespace RattusAPI.Authentication
{
    public interface IAuthenticationProvider : IProvider
    {
        ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context);
    }
}
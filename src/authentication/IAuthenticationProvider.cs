using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using LobbyAPI.Provider;

namespace LobbyAPI.Authentication
{
    public interface IAuthenticationProvider : IProvider
    {
        ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context);
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LobbyAPI.Authentication
{
    public class JWTAuthenticationProvider : IAuthenticationProvider
    {
        public string RegisteredName => "JWT";

        public ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
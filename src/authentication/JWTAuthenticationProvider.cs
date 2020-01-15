using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RattusAPI.Authentication
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
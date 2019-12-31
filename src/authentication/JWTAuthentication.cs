using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RattusAPI.Authentication
{
    public class JWTAuthentication : IAuthentication
    {
        public ClaimsPrincipal AuthenticateUser(AuthorizationFilterContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}